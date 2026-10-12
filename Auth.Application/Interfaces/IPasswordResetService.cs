using Auth.Application.DTOs.PasswordReset;
using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.Application.Interfaces
{
    public interface IPasswordResetService
    {
        Task ResetPasswordAsync(
       ResetPasswordRequest request,
       CancellationToken cancellationToken = default);

        Task RequestPasswordResetAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default);
    }
}
