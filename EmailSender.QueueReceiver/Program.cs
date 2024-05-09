using Azure.Messaging.ServiceBus;
using EmailSender.Concerns;
using EmailSender.Service;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("C:\\Users\\Nitish.d\\source\\repos\\EmailSenderWebApi\\EmailSender.QueueReceiver\\appsettings.json")
    .Build();

ServiceBusClient serviceBusClient = new ServiceBusClient(configuration["AzureServiceBus:ConnectionString"]);
ServiceBusProcessor processor = serviceBusClient.CreateProcessor(configuration["AzureServiceBus:QueueName"], new ServiceBusProcessorOptions());

try
{
    processor.ProcessMessageAsync += MessageHandler;
    processor.ProcessErrorAsync += ErrorHandler;

    await processor.StartProcessingAsync();

    Console.WriteLine("Wait for a minute and then press any key to end the processing");
    Console.ReadKey();

    Console.WriteLine("\nStopping the receiver");
    await processor.StopProcessingAsync();
    Console.WriteLine("Stopped receiving messages");
}
finally
{
    await processor.DisposeAsync();
    await serviceBusClient.DisposeAsync();
}

async Task MessageHandler(ProcessMessageEventArgs args)
{
    var smtpConfig = configuration.GetSection("SmtpConfig").Get<SmtpConfig>();
    var emailAgent = new EmailAgent(smtpConfig);
    var emailService = new EmailService(emailAgent,10);
    string emailMessageJson = args.Message.Body.ToString();
    var emailMessage = JsonSerializer.Deserialize<EmailMessage[]>(emailMessageJson);
    await emailService.SendEmailsInBatchesAsync(emailMessage);
    await args.CompleteMessageAsync(args.Message);
}

Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine(args.Exception.ToString());
    return Task.CompletedTask;
}