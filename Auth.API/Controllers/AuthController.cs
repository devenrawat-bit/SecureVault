using Auth.Application.DTOs.Authentication;
using Auth.Application.DTOs.PasswordReset;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IPasswordResetService _passwordResetService;

        public AuthController(IAuthService authService, IPasswordResetService passwordResetService )
        {
            _authService = authService;
            _passwordResetService = passwordResetService;
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(
           [FromBody] ForgotPasswordRequest request,
           CancellationToken cancellationToken)
        {
            try
            {
                await _passwordResetService.RequestPasswordResetAsync(
                    request,
                    cancellationToken);

                // Return the same response whether or not the account exists.
                return Ok(new
                {
                    message = "If an account exists for that email, password reset instructions will be sent."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while processing the request."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                await _passwordResetService.ResetPasswordAsync(
                    request,
                    cancellationToken);

                return Ok(new
                {
                    message = "Password has been reset successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new
                {
                    message = "The password reset request is invalid or has expired, or the password does not meet the requirements."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    message = "An unexpected error occurred while resetting the password."
                });
            }
        }
    }
}
