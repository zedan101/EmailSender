using Azure.Messaging.ServiceBus;
using EmailSender.Concerns;
using EmailSender.Service;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("C:\\Users\\Nitish.d\\source\\repos\\EmailSenderWebApi\\EmailSender.TopicReceiver1\\appsettings.json")
    .Build();

// Get Service Bus connection string and topic information
string serviceBusConnectionString = configuration["AzureServiceBus:ConnectionString"];
string topicName = configuration["AzureServiceBus:TopicName"];
string subscriptionName = configuration["AzureServiceBus:SubscriptionName"];

ServiceBusClient serviceBusClient = new ServiceBusClient(serviceBusConnectionString);
ServiceBusProcessor processor = serviceBusClient.CreateProcessor(
    topicName,
    subscriptionName,
    new ServiceBusProcessorOptions()
);

try
{
    processor.ProcessMessageAsync += MessageHandler;
    processor.ProcessErrorAsync += ErrorHandler;

    // Start processing messages
    await processor.StartProcessingAsync();

    Console.WriteLine("Waiting for messages. Press any key to stop...");
    Console.ReadKey();

    Console.WriteLine("\nStopping the processor...");
    await processor.StopProcessingAsync();
    Console.WriteLine("Processor stopped.");
}
finally
{
    // Dispose of resources properly
    await processor.DisposeAsync();
    await serviceBusClient.DisposeAsync();
}

// Message handler for processing incoming messages
async Task MessageHandler(ProcessMessageEventArgs args)
{
    var smtpConfig = configuration.GetSection("SmtpConfig").Get<SmtpConfig>();
    var emailAgent = new EmailAgent(smtpConfig);
    var emailService = new EmailService(emailAgent, 10);

    // Get message body and deserialize it
    string emailMessageJson = args.Message.Body.ToString();
    var emailMessages = JsonSerializer.Deserialize<EmailMessage[]>(emailMessageJson);

    // Send emails in batches
    await emailService.SendEmailsInBatchesAsync(emailMessages);

    // Complete the message
    await args.CompleteMessageAsync(args.Message);
}

// Error handler for dealing with errors during processing
Task ErrorHandler(ProcessErrorEventArgs args)
{
    Console.WriteLine($"Error: {args.Exception.Message}");
    return Task.CompletedTask;
}
