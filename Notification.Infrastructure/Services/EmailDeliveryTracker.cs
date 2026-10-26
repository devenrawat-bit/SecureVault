using Microsoft.EntityFrameworkCore;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Infrastructure.Services
{
    public class EmailDeliveryTracker : IEmailDeliveryTracker
    {
        #region Constructor and Private variables
        private readonly NotificationDbContext _notificationDbContext;
        public EmailDeliveryTracker(NotificationDbContext notificationDbContext)
        {
            _notificationDbContext = notificationDbContext;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Determines whether a notification can be processed.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="userId"></param>
        /// <param name="recipientEmail"></param>
        /// <param name="notificationType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> CanProcessNotificationAsync(string messageId, Guid? userId, string recipientEmail, string notificationType, CancellationToken cancellationToken)
        {
            // Check whether a delivery record already exists for this message.
            var existingRecord = await _notificationDbContext.EmailDeliveries.FirstOrDefaultAsync(delivery => delivery.MessageId == messageId, cancellationToken);
            // If the email was already sent, do not send it again.

             if (existingRecord?.Status == "Sent") { return false; } 
             // If another attempt is already pending, do not process the same message again.

             if (existingRecord?.Status == "Pending") { return false; }

             // If the previous attempt failed, prepare the existing record for a retry.
             if (existingRecord?.Status == "Failed") { existingRecord.Status = "Pending"; existingRecord.AttemptCount++; existingRecord.LastError = null; 
                await _notificationDbContext.SaveChangesAsync(cancellationToken); return true; }

             // Create a new delivery record when this message has never been processed.
             var deliveryRecord = new EmailDelivery { MessageId = messageId, UserId = userId, RecipientEmail = recipientEmail, NotificationType = notificationType, Status = "Pending", AttemptCount = 1 };
             _notificationDbContext.EmailDeliveries.Add(deliveryRecord);
            await _notificationDbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        /// <summary>
        /// Marks an email delivery as failed.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="error"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task MarkFailedAsync(string messageId, string error, CancellationToken cancellationToken)
        {
            // Find the delivery record for this message.
            var deliveryRecord = await _notificationDbContext.EmailDeliveries.FirstOrDefaultAsync(delivery => delivery.MessageId == messageId, cancellationToken);
            // Do not update anything if the delivery record cannot be found.
            if (deliveryRecord == null) { return; } 
            // Mark the delivery as failed and store a limited error description.
            deliveryRecord.Status = "Failed";
            deliveryRecord.LastError = error.Length > 1000 ? error[..1000] : error; 
            await _notificationDbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Marks an email delivery as sent.
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task MarkSentAsync(string messageId, CancellationToken cancellationToken)
        {
            // Find the delivery record for this message.
            var deliveryRecord = await _notificationDbContext.EmailDeliveries.FirstOrDefaultAsync( delivery => delivery.MessageId == messageId, cancellationToken); 
            // Do not update anything if the delivery record cannot be found.
            if (deliveryRecord == null) { return; } 
            // Mark the delivery as successfully submitted to the email sender.
            deliveryRecord.Status = "Sent"; 
            deliveryRecord.SentAtUtc = DateTime.UtcNow; deliveryRecord.LastError = null;
            await _notificationDbContext.SaveChangesAsync(cancellationToken);
        }
        #endregion
    }
}
