using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Interfaces
{
    /// <summary>
    /// This interface is used to send the emails.
    /// </summary>
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    }
}
