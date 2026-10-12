using Notification.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Notification.Application.Interfaces
{
    public interface IInvitationEmailService
    {
        Task SendAdminInvitationAsync(
        AdminInvitationEmailRequest request,
        CancellationToken cancellationToken = default);
    }
}
