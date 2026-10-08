using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Interfaces;

namespace Notification.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailTestController : ControllerBase
    {
        private readonly IEmailSender _emailSender;
        public EmailTestController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        /// <summary>
        /// This is a test endpoint to send a test email to the specified email address. It is used to verify that the SMTP integration is working correctly.
        /// </summary>
        /// <param name="email">To whom the test email should be sent</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A task representing the asynchronous operation</returns>
        [HttpPost("test")]
        public async Task<IActionResult> SendTestEmail(
       [FromQuery] string email,
       CancellationToken cancellationToken)
        {
            await _emailSender.SendAsync(
                email,
                "SecureVault Test Email",
                "<h1>SMTP integration is working!</h1><p>This is a test email from Notification Service.</p>",
                cancellationToken);

            return Ok(new
            {
                Success = true,
                Message = "Test email sent successfully."
            });
        }
    }
}
