using Microsoft.AspNetCore.Identity.UI.Services;

namespace MyCourse.Models.Services.Infrastructure;
public interface IEmailClient : IEmailSender
{
    Task SendEmailAsync(string recipientEmail, string replyToMail, string subject, string htmlMessage);
}
