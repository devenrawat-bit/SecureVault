using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Infrastructure.Services
{
    public class InvitationEmailService:IInvitationEmailService
    {
        private readonly IEmailSender _emailSender;
        public InvitationEmailService(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task SendAdminInvitationAsync(AdminInvitationEmailRequest request, CancellationToken cancellationToken = default)
        {
            var subject = $"Invitation to join {request.OrganizationName}";

            var htmlBody = $"""
            <html>
            <body>
                <h2>Hello {request.RecipientName},</h2>

                <p>
                    You have been invited to become an administrator
                    of <strong>{request.OrganizationName}</strong>.
                </p>

                <p>
                    Click the button below to accept your invitation
                    and complete your account setup.
                </p>

                <p>
                    <a href="{request.InvitationLink}"
                       style="
                           display:inline-block;
                           padding:10px 20px;
                           background-color:#007bff;
                           color:white;
                           text-decoration:none;
                           border-radius:5px;">
                        Accept Invitation
                    </a>
                </p>

                <p>
                    This invitation is valid for 24 hours.
                </p>

                <p>
                    If you did not expect this invitation, you can ignore this email.
                </p>

                <p>
                    Regards,<br />
                    SecureVault Team
                </p>
            </body>
            </html>
            """;

            await _emailSender.SendAsync(
                request.RecipientEmail,
                subject,
                htmlBody,
                cancellationToken);
        }
    }
}
