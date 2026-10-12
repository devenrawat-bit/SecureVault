using System;
using System.Collections.Generic;
using System.Text;

namespace Auth.Application.DTOs.PasswordReset
{
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}
