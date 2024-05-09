using EmailSender.Concerns;
using EmailSender.Contracts;

namespace EmailSender.Service
{
    public class EmailService : IEmailService
    {
        private readonly IEmailAgent _emailAgent;
        private readonly int _batchSize;
        public EmailService()
        {
        }
        public EmailService(IEmailAgent emailAgent, int batchSize)
        {
            _emailAgent = emailAgent;
            _batchSize = batchSize;
        }

        public async Task SendEmailsInBatchesAsync(IEnumerable<EmailMessage> emailMessages)
        {
            var emailList = new List<EmailMessage>(emailMessages);
            var totalEmails = emailList.Count;

            for (int i = 0; i < totalEmails; i += _batchSize)
            {
                var batch = emailList.GetRange(i, Math.Min(_batchSize, totalEmails - i));
                var tasks = new List<Task>();

                foreach (var email in batch)
                {
                    tasks.Add(_emailAgent.SendEmailAsync(email.To, email.Subject, email.Body));
                }

                await Task.WhenAll(tasks);
                
            }
        }
    }
}
