using Auth.Application.DTOs.Invitations;
using Auth.Application.DTOs.PasswordReset;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.Application.Interfaces
{
    /// <summary>
    /// These are the methods that are used for the RabbitMQ publisher
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// This mehtod is used to publish an invitation created event to the message bus from the organization service to the notification service.
        /// </summary>
        /// <param name="invitationEvent">The data that will be sent to the notification service </param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task PublishInvitationCreatedAsync(
        InvitationCreatedMessage invitationEvent,
        CancellationToken cancellationToken = default);

        /// <summary>
        /// Publish a password-reset request to RabbitMQ for the Notification Service.
        /// </summary>
        /// <param name="message">The data that will be sent to the notification service</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task PublishPasswordResetRequestedAsync(
            PasswordResetRequestedMessage message,
            CancellationToken cancellationToken = default);
    }
}
