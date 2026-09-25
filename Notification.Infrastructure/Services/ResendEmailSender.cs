using Microsoft.Extensions.Options;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Configuration;
using Resend;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Infrastructure.Services
{
    /// <summary>
    /// This class is used to send the emails using the Resend email service.
    /// </summary>
    public class ResendEmailSender : IEmailSender
    {
        #region Private Fields
        private readonly ResendOptions _options;
        private readonly IResend _resend; //provided by the resend package
        #endregion

        #region Constructor
        public ResendEmailSender(IOptions<ResendOptions> options, IResend resend)
        {
            _options = options.Value; //this ioptions will bind the hardcoded ResendOptions class with the values from the appsettings.json file
            _resend = resend;
        }
        #endregion

        #region Methods

        /// <summary>
        /// This method is used to send the email using the Resend email service.
        /// </summary>
        /// <param name="to">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="htmlBody">The HTML body of the email.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            var message = new EmailMessage //this is provided by the resend package
            {
                From = $"{_options.FromName} <{_options.FromEmail}>",
                Subject = subject,
                HtmlBody = htmlBody
            };
            message.To.Add(to);

            await _resend.EmailSendAsync(message); //here the emailsendasync method is inbuilt and provided by the resend package that we did from nuget package manager
        }
        #endregion
    }
}
