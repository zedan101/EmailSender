

namespace EmailSender.Contracts
{
    public interface IEmailAgent
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
