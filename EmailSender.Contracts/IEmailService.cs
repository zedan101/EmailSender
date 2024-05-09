using EmailSender.Concerns;


namespace EmailSender.Contracts
{
    public interface IEmailService
    {
        Task SendEmailsInBatchesAsync(IEnumerable<EmailMessage> emailMessages);
    }
}
