using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
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
        
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;

        public ApplicationsController(ILogger<ApplicationsController> logger)
        {
            _logger = logger;
            
            var factory = new ConnectionFactory { HostName = "localhost"};
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);
            
            props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var response = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    respQueue.Add(response);
                }
            };
        }

        [HttpPost]
        public IActionResult AddApplication(AddApplicationCommand command)
        {
            _logger.LogInformation("Add application: {0}", JsonConvert.SerializeObject(command));
            
            channel.QueueDeclare(queue: "AddApplicationQueue",
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

            var message = JsonConvert.SerializeObject(mqCommand);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                routingKey: "AddApplicationQueue",
                basicProperties: props,
                body: body);
            
            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);

            return Ok(respQueue.Take());
        }
        
        [HttpGet("ByRequestId")]
        public IActionResult GetApplicationStatus(GetApplicationByRequestIdCommand command)
        {
            _logger.LogInformation("Get application status with request id: {0}", command.RequestId);
            
            channel.QueueDeclare(queue: "GetByRequestQueue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var mqCommand = new GetApplicationByRequestIdMqCommand
            {
                RequestId = command.RequestId,
                ClientIp = HttpContext.Connection.RemoteIpAddress.ToString(),
            };
            
            var message = JsonConvert.SerializeObject(mqCommand);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                routingKey: "GetByRequestQueue",
                basicProperties: props,
                body: body);

            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);

            return Ok(respQueue.Take());
        }
        
        [HttpGet("ByClientId")]
        public IActionResult GetApplicationStatus(GetApplicationByClientIdCommand command)
        {
            _logger.LogInformation("Get application status with client id {0}, department address {1}", command.ClientId, command.DepartmentAddress);
            
            channel.QueueDeclare(queue: "GetByClientQueue",
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
            
            var message = JsonConvert.SerializeObject(mqCommand);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                routingKey: "GetByClientQueue",
                basicProperties: props,
                body: body);

            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);

            return Ok(respQueue.Take());
        }
    }
}