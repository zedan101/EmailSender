using Azure.Messaging.ServiceBus;
using EmailSender.Concerns;
using EmailSender.Contracts;
using EmailSender.Service;
using Hangfire;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration["Hangfire:ConnectionString"])
);

builder.Services.AddHangfireServer();

builder.Services.Configure<SmtpConfig>(builder.Configuration.GetSection("SmtpConfig"));
builder.Services.AddSingleton<IEmailAgent>(sp =>
{
    var smtpConfig = sp.GetRequiredService<IOptions<SmtpConfig>>().Value;
    return new EmailAgent(smtpConfig);
});

builder.Services.AddSingleton<IEmailService>(sp =>
{
    var emailAgent = sp.GetRequiredService<IEmailAgent>();
    return new EmailService(emailAgent, 10); 
});

// Get the Service Bus connection string
var serviceBusConnectionString = builder.Configuration.GetSection("AzureServiceBus:ConnectionString").Value;

// Add ServiceBusClient as a singleton service
builder.Services.AddSingleton(sp =>
{
    return new ServiceBusClient(serviceBusConnectionString);
});

// Setup queue sender
var queueName = builder.Configuration.GetSection("AzureServiceBus:QueueName").Value;
builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<ServiceBusClient>();
    return client.CreateSender(queueName);
});

// Setup topic sender
var topicName = builder.Configuration.GetSection("AzureServiceBus:TopicName").Value; // New line
builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<ServiceBusClient>();
    return client.CreateSender(topicName); // New line
});

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHangfireDashboard();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHangfireDashboard();
});

app.Run();
