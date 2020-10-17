using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApplicationsController : ControllerBase
    {
        private readonly ILogger<ApplicationsController> _logger;
        private readonly IModel _channel;
        private readonly string _replyQueueName;
        private readonly EventingBasicConsumer _consumer;
        private readonly BlockingCollection<string> _respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties _props;
        
        private const string AddQueueName = "AddApplicationQueue";
        private const string GetByRequestIdQueueName = "GetByRequestQueue";
        private const string GetByClientIdQueueName = "GetByClientQueue";

        public ApplicationsController(ILogger<ApplicationsController> logger, IConnection connection)
        {
            _logger = logger;
            _channel = connection.CreateModel();

            _replyQueueName = _channel.QueueDeclare().QueueName;
            _consumer = new EventingBasicConsumer(_channel);
            
            _props = _channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            _props.CorrelationId = correlationId;
            _props.ReplyTo = _replyQueueName;

            _consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var response = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    _respQueue.Add(response);
                }
            };
        }

        [HttpPost]
        public IActionResult AddApplication(AddApplicationCommand command)
        {
            if (string.IsNullOrEmpty(command.Currency)
                || string.IsNullOrEmpty(command.ClientId)
                || string.IsNullOrEmpty(command.DepartmentAddress)
                || command.Amount == 0)
            {
                _logger.LogError("Request is invalid");
                return BadRequest();
            }
            
            _logger.LogInformation($"Processing request: {JsonConvert.SerializeObject(command)}");
            
            _channel.QueueDeclare(queue: AddQueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var mqCommand = new AddApplicationMqCommand
            {
                ClientId = command.ClientId,
                Amount = command.Amount,
                Currency = command.Currency,
                DepartmentAddress = command.DepartmentAddress,
                ClientIp = HttpContext.Connection.RemoteIpAddress.ToString(),
            };

            string message;
            try
            {
                message = JsonConvert.SerializeObject(mqCommand);
                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublish(exchange: "",
                    routingKey: AddQueueName,
                    basicProperties: _props,
                    body: body);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error from message queue: {e.Message}");
                return StatusCode((int)HttpStatusCode.BadGateway);
            }
            
            _logger.LogInformation($"Sent message: {message}" );

            _channel.BasicConsume(
                consumer: _consumer,
                queue: _replyQueueName,
                autoAck: true);
            
            var response = _respQueue.Take();
           
            _logger.LogInformation($"Received message from server: {response}");
            return Ok(response);
        }
        
        [HttpGet("ByRequestId")]
        public IActionResult GetApplicationStatus(GetApplicationByRequestIdCommand command)
        {
            if (string.IsNullOrEmpty(command.RequestId))
            {
                _logger.LogError("Request is invalid");
                return NotFound();
            }
            _logger.LogInformation($"Processing request: {JsonConvert.SerializeObject(command)}");
            
            _channel.QueueDeclare(queue: GetByRequestIdQueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var mqCommand = new GetApplicationByRequestIdMqCommand
            {
                RequestId = command.RequestId,
                ClientIp = HttpContext.Connection.RemoteIpAddress.ToString(),
            };

            string message;
            
            try
            {
                message = JsonConvert.SerializeObject(mqCommand);
                var body = Encoding.UTF8.GetBytes(message);
                _channel.BasicPublish(exchange: "",
                    routingKey: GetByRequestIdQueueName,
                    basicProperties: _props,
                    body: body);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error from message queue: {e.Message}");
                return StatusCode((int)HttpStatusCode.BadGateway);
            }
            
            _logger.LogInformation($"Sent message: {message}");
            
            _channel.BasicConsume(
                consumer: _consumer,
                queue: _replyQueueName,
                autoAck: true);

            var response = _respQueue.Take();
           
            _logger.LogInformation($"Received message from server: {response}");
            return Ok(response);
        }
        
        [HttpGet("ByClientId")]
        public IActionResult GetApplicationStatus(GetApplicationByClientIdCommand command)
        {
            _logger.LogInformation($"Processing request: {JsonConvert.SerializeObject(command)}");
            
            if (string.IsNullOrEmpty(command.ClientId) || string.IsNullOrEmpty(command.DepartmentAddress))
            {
                _logger.LogError("Request is invalid");
                return NotFound();
            }
            
            _channel.QueueDeclare(queue: GetByClientIdQueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var mqCommand = new GetApplicationByClientIdMqCommand()
            {
                ClientId = command.ClientId,
                DepartmentAddress = command.DepartmentAddress,
                ClientIp = HttpContext.Connection.RemoteIpAddress.ToString(),
            };
            
            string message;
            
            try
            {
                message = JsonConvert.SerializeObject(mqCommand);
                var body = Encoding.UTF8.GetBytes(message);
                _channel.BasicPublish(exchange: "",
                    routingKey: GetByClientIdQueueName,
                    basicProperties: _props,
                    body: body);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error from message queue: {e.Message}");
                return StatusCode((int)HttpStatusCode.BadGateway);
            }
            _logger.LogInformation($"Sent message: {message}");
                
            _channel.BasicConsume(
                consumer: _consumer,
                queue: _replyQueueName,
                autoAck: true);

            var response = _respQueue.Take();
           
            _logger.LogInformation($"Received message from server: {response}");
            return Ok(response);
        }
    }
}