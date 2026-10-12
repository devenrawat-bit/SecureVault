using Notification.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Infrastructure.Services
{
    public sealed class PasswordResetEmailService : IPasswordResetEmailService //no inheritance of this class will be now allowed by the sealed keyword 
    {
        public Task SendPasswordResetEmailAsync(Guid userId, string recipientEmail, string resetLink, CancellationToken cancellationToken = default)
        {
           
        }
    }
}
