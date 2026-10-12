namespace Auth.Application.DTOs.PasswordReset;

/// <summary>
/// This class will be used for the RabbitMQ serivce 
/// </summary>
public sealed class PasswordResetRequestedMessage
{
    public Guid UserId { get; set; }

    public string RecipientEmail { get; set; } = string.Empty;

    public string ResetLink { get; set; } = string.Empty;
}