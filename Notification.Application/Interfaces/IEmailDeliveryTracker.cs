namespace Notification.Application.Interfaces;

public interface IEmailDeliveryTracker
{
    #region Interfaces
    /// <summary>
    /// To prevent that the rabbitMQ dont send the same email multiple times, we will create a record in the database to track the delivery status of the email.
    /// If no record exists for the given messageId, create a new record with pending status and return true.
    /// If record exists with sent status , return false.
    /// If record exists with pending status, return false.
    /// If record exists with failed status, return true to allow retrying the email delivery.
    /// </summary>
    /// <param name="messageId">Uniquely identifies a notification message</param>
    /// <param name="userId">Identifies the user receiving the notification</param>
    /// <param name="recipientEmail">Email address receiving the notification</param>
    /// <param name="notificationType">Type of notification</param>
    /// <param name="cancellationToken">Allows the operation to be cancelled</param>
    /// <returns>True or false, if true mean it is eligible to be sent if false it mean this should be skipped now  </returns>
    /// 
    Task<bool> CanProcessNotificationAsync(
    string messageId,
    Guid? userId,
    string recipientEmail,
    string notificationType,
    CancellationToken cancellationToken);

    /// <summary>
    /// Updates the delivery record after successful email submission.
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task MarkSentAsync(
        string messageId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates the delivery record after a failed attempt.
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task MarkFailedAsync(
        string messageId,
        string error,
        CancellationToken cancellationToken);
    #endregion
}
