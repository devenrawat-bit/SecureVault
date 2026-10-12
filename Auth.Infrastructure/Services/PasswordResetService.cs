using Auth.Application.DTOs.PasswordReset;
using Auth.Application.Interfaces;
using Auth.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text;

namespace Auth.Infrastructure.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly string _frontendBaseUrl;

        public PasswordResetService(
            UserManager<ApplicationUser> userManager,
            IEventPublisher eventPublisher,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _eventPublisher = eventPublisher;

            // Read the frontend URL from configuration.
            _frontendBaseUrl =
                configuration["Frontend:BaseUrl"]
                ?? throw new InvalidOperationException(
                    "Frontend:BaseUrl configuration is missing.");
        }

        public async Task RequestPasswordResetAsync(
            ForgotPasswordRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Find the user using the supplied email address.
                var user = await _userManager.FindByEmailAsync(request.Email);

                // Return without revealing whether the account exists.
                if (user is null)
                {
                    return;
                }

                // Generate a password-reset token using ASP.NET Core Identity.
                var identityToken =
                    await _userManager.GeneratePasswordResetTokenAsync(user);

                // Encode the token so it can safely be included in a URL.
                var encodedToken = WebEncoders.Base64UrlEncode(
                    Encoding.UTF8.GetBytes(identityToken));

                // Build the reset link using the configured frontend URL.
                var resetLink =
                    $"{_frontendBaseUrl.TrimEnd('/')}/reset-password" +
                    $"?email={Uri.EscapeDataString(request.Email)}" +
                    $"&token={Uri.EscapeDataString(encodedToken)}";

                // The encoded token must be delivered to the user through the Notification Service.
                // Do not log the token or return it from the production API.

                // Create the message that will be sent to the Notification Service.
                var resetMessage = new PasswordResetRequestedMessage
                {
                    UserId = user.Id,
                    RecipientEmail = user.Email ?? request.Email,
                    ResetLink = resetLink
                };

                // Publish the password-reset event to RabbitMQ.
                await _eventPublisher.PublishPasswordResetRequestedAsync(
                    resetMessage,
                    cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Do not log the reset token or the user's password.
                throw new InvalidOperationException(
                    "An unexpected error occurred while requesting a password reset.",
                    ex);
            }
        }

        public async Task ResetPasswordAsync(
            ResetPasswordRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check that the new password and confirmation match.
                if (request.NewPassword != request.ConfirmPassword)
                {
                    throw new ArgumentException(
                        "The new password and confirmation do not match.");
                }

                // Find the account associated with the email address.
                var user = await _userManager.FindByEmailAsync(request.Email);

                if (user is null)
                {
                    throw new InvalidOperationException(
                        "The password reset request is invalid or has expired.");
                }

                // Decode the URL-safe token into the original Identity token.
                string decodedToken;

                try
                {
                    var tokenBytes = WebEncoders.Base64UrlDecode(request.Token);
                    decodedToken = Encoding.UTF8.GetString(tokenBytes);
                }
                catch (FormatException)
                {
                    throw new InvalidOperationException(
                        "The password reset request is invalid or has expired.");
                }

                // Validate the token and update the password.
                // ASP.NET Core Identity also applies the configured password policy.
                var result = await _userManager.ResetPasswordAsync(
                    user,
                    decodedToken,
                    request.NewPassword);

                if (!result.Succeeded)
                {
                    // Collect the validation errors returned by ASP.NET Core Identity.
                    var errors = string.Join(
                        "; ",
                        result.Errors.Select(error => error.Description));

                    throw new InvalidOperationException(
                        $"Password reset failed: {errors}");
                }
            }
            catch (ArgumentException)
            {
                // Preserve password-confirmation validation errors.
                throw;
            }
            catch (InvalidOperationException)
            {
                // Preserve known password-reset errors.
                throw;
            }
            catch (Exception ex)
            {
                // Log the exception in production using ILogger.
                // Do not log the reset token or the user's password.
                throw new InvalidOperationException(
                    "An unexpected error occurred while resetting the password.",
                    ex);
            }
        }
    }
}