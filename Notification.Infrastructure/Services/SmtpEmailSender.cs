using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Configuration;

namespace Notification.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        var email = new MimeMessage();

        // Sender
        email.From.Add(
            new MailboxAddress(
                _options.FromName,
                _options.FromEmail));

        // Recipient
        email.To.Add(MailboxAddress.Parse(to));

        // Subject
        email.Subject = subject;

        // HTML body
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };

        email.Body = bodyBuilder.ToMessageBody();

        using var smtpClient = new SmtpClient();

        // Connect to Gmail SMTP server
        await smtpClient.ConnectAsync(
            _options.Host,
            _options.Port,
            SecureSocketOptions.StartTls,
            cancellationToken);

        // Authenticate using Gmail + App Password
        await smtpClient.AuthenticateAsync(
            _options.Username,
            _options.Password,
            cancellationToken);

        // Send email
        await smtpClient.SendAsync(
            email,
            cancellationToken);

        // Disconnect
        await smtpClient.DisconnectAsync(
            true,
            cancellationToken);
    }
}