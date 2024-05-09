using EmailSender.Concerns;
using EmailSender.Contracts;
using MailKit.Net.Smtp;
using MimeKit;

namespace EmailSender.Service
{
    public class EmailAgent : IEmailAgent
    {

        private readonly SmtpConfig _config;

        public EmailAgent(SmtpConfig config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Zedan", _config.Username));
            message.To.Add(new MailboxAddress("Client", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { TextBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_config.Host, _config.Port, _config.UseSsl ? MailKit.Security.SecureSocketOptions.Auto : MailKit.Security.SecureSocketOptions.None);
                // await client.ConnectAsync(_config.Host, _config.Port, false);
                await client.AuthenticateAsync(_config.Username, _config.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
