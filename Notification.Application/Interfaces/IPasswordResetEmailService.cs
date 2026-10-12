using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Interfaces
{
    public interface IPasswordResetEmailService
    {
        Task SendPasswordResetEmailAsync(
       Guid userId,
       string recipientEmail,
       string resetLink,
       CancellationToken cancellationToken = default);
    }
}
