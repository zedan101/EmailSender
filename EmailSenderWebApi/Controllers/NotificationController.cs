using Azure.Messaging.ServiceBus;
using EmailSender.Concerns;
using EmailSender.Contracts;
using EmailSender.Service;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Text.Json;

namespace EmailSender.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly ServiceBusSender _queueSender;
        private readonly ServiceBusSender _topicSender;
        private readonly IEmailService _emailService;

        public NotificationController(ServiceBusClient serviceBusClient, IEmailService emailService)
        {
            _queueSender = serviceBusClient.CreateSender("nitish");
            _topicSender = serviceBusClient.CreateSender("smtp_email_topic");
            _emailService = emailService;
        }

        [HttpPost("SendEmailUsingQueue")]
        public async Task<IActionResult> SendEmailUsingQueueAsync( EmailMessage[] recieversData)
        {
            try
            {
                var messageContent = JsonSerializer.Serialize(recieversData);
                var message = new ServiceBusMessage(messageContent);

                await _queueSender.SendMessageAsync(message);

                return Ok("Email message sent to queue.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error sending email message to queue: {ex.Message}");
            }
        }

        [HttpPost("SendEmailUsingTopic")]
        public async Task<IActionResult> SendEmailUsingTopicAsync(EmailMessage[] recieversData)
        {
            try
            {
                var messageContent = JsonSerializer.Serialize(recieversData);
                var message = new ServiceBusMessage(messageContent);

                await _topicSender.SendMessageAsync(message);

                return Ok("Email message sent to queue.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error sending email message to queue: {ex.Message}");
            }
        }

        [HttpPost("SendEmailToBackgroundJob")]
        public async Task<IActionResult> SendEmailToBGJobAsync(EmailMessage[] recieversData)
        {
            try
            {
                BackgroundJob.Enqueue<IEmailService>(x => x.SendEmailsInBatchesAsync(recieversData));
                return Ok("Email sent to background job");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error sending email message to queue: {ex.Message}");
            }
        }
    }
}
