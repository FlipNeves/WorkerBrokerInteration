using Microsoft.AspNetCore.Mvc;
using WorkerBrokerInteration.Domain.Interfaces;
using WorkerBrokerInteration.Domain.Models;

namespace WorkerBrokerInteration.Controller
{
    /// <summary>
    /// Test controller to demonstrate publishing messages to RabbitMQ queues.
    /// In production, publishing is typically done inside Services or Jobs.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class QueueTestController : ControllerBase
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<QueueTestController> _logger;

        public QueueTestController(IEventPublisher eventPublisher, ILogger<QueueTestController> logger)
        {
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        /// <summary>
        /// Sends a simple message (text/ID) to the SAMPLE_QUEUE_EVENT queue.
        /// 
        /// Example call:
        ///   POST /api/QueueTest/send?message=12345
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromQuery] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return BadRequest("Message cannot be empty.");

            var request = new MessageRequestModel
            {
                QueueEvent = QueueEventType.SAMPLE_QUEUE_EVENT,
                MessageType = MessageType.RESOURCE_UNIQUE_ID,
                Message = message,
                Reprocess = false
            };

            await _eventPublisher.SendAsync(request);

            _logger.LogInformation("Message sent to queue {Queue}: {Message}",
                QueueEventType.SAMPLE_QUEUE_EVENT, message);

            return Ok(new
            {
                Status = "Message sent successfully",
                Queue = QueueEventType.SAMPLE_QUEUE_EVENT.ToString(),
                Message = message
            });
        }

        /// <summary>
        /// Sends a message with a JSON body to the SAMPLE_QUEUE_EVENT queue.
        /// 
        /// Example call:
        ///   POST /api/QueueTest/send-body
        ///   Body: { "name": "Test User", "email": "test@example.com" }
        /// </summary>
        [HttpPost("send-body")]
        public async Task<IActionResult> SendBodyMessage([FromBody] object body)
        {
            if (body == null)
                return BadRequest("Body cannot be null.");

            var serialized = System.Text.Json.JsonSerializer.Serialize(body);

            var request = new MessageRequestModel
            {
                QueueEvent = QueueEventType.SAMPLE_QUEUE_EVENT,
                MessageType = MessageType.RESOURCE_BODY,
                Message = serialized,
                Reprocess = false
            };

            await _eventPublisher.SendAsync(request);

            return Ok(new
            {
                Status = "Message sent successfully",
                Queue = QueueEventType.SAMPLE_QUEUE_EVENT.ToString(),
                MessageType = MessageType.RESOURCE_BODY.ToString(),
                Payload = serialized
            });
        }

        /// <summary>
        /// Sends multiple messages at once (batch) to the SAMPLE_QUEUE_EVENT queue.
        /// 
        /// Example call:
        ///   POST /api/QueueTest/send-batch
        ///   Body: ["msg1", "msg2", "msg3"]
        /// </summary>
        [HttpPost("send-batch")]
        public async Task<IActionResult> SendBatchMessages([FromBody] List<string> messages)
        {
            if (messages == null || !messages.Any())
                return BadRequest("Messages list cannot be empty.");

            var requests = messages.Select(msg => new MessageRequestModel
            {
                QueueEvent = QueueEventType.SAMPLE_QUEUE_EVENT,
                MessageType = MessageType.RESOURCE_UNIQUE_ID,
                Message = msg,
                Reprocess = false
            }).ToList();

            await _eventPublisher.SendAllAsync(requests);

            return Ok(new
            {
                Status = $"{messages.Count} messages sent successfully",
                Queue = QueueEventType.SAMPLE_QUEUE_EVENT.ToString(),
                Messages = messages
            });
        }

        /// <summary>
        /// Sends a message with the reprocess flag enabled.
        /// Useful for retry/dead-letter scenarios.
        /// 
        /// Example call:
        ///   POST /api/QueueTest/send-reprocess?message=12345
        /// </summary>
        [HttpPost("send-reprocess")]
        public async Task<IActionResult> SendReprocessMessage([FromQuery] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return BadRequest("Message cannot be empty.");

            var request = new MessageRequestModel
            {
                QueueEvent = QueueEventType.SAMPLE_QUEUE_EVENT,
                MessageType = MessageType.RESOURCE_UNIQUE_ID,
                Message = message,
                Reprocess = true
            };

            await _eventPublisher.SendAsync(request);

            return Ok(new
            {
                Status = "Reprocess message sent",
                Queue = QueueEventType.SAMPLE_QUEUE_EVENT.ToString(),
                Message = message,
                Reprocess = true
            });
        }
    }
}
