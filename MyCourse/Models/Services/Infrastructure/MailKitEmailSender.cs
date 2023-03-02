using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MyCourse.Models.Options;

namespace MyCourse.Models.Services.Infrastructure
{
    public class MailKitEmailSender : IEmailClient
    {
        private readonly IOptionsMonitor<SmtpOptions> smtpOptions;
        private readonly ILogger<MailKitEmailSender> logger;

        public MailKitEmailSender(IOptionsMonitor<SmtpOptions> emailOptions, ILogger<MailKitEmailSender> logger)
        {
            this.logger = logger;
            this.smtpOptions = emailOptions;
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return SendEmailAsync(email, null, subject, htmlMessage);
        }

        public async Task SendEmailAsync(string recipientEmail, string replyToMail, string subject, string htmlMessage)
        {
            try
            {
                SmtpOptions options = smtpOptions.CurrentValue;
                using var client = new SmtpClient();
                await client.ConnectAsync(options.Host, options.Port, options.Security);
                if(!string.IsNullOrEmpty(options.Username))
                {
                    await client.AuthenticateAsync(options.Username, options.Password);
                }

                var message = new MimeMessage();
                message.From.Add(MailboxAddress.Parse(options.Sender));
                message.To.Add(MailboxAddress.Parse(recipientEmail));
                if(replyToMail is not (null or ""))
                {
                    message.ReplyTo.Add(MailboxAddress.Parse(replyToMail));
                }
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = htmlMessage };
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception exc)
            {
                logger.LogError(exc, "Couldn't send email to {email} with message {message}", recipientEmail, htmlMessage);
            }
        }
    }
}
