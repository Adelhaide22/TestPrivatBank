using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult AddApplication(AddApplicationCommand command)
        {
            _logger.LogInformation("Add application: {0}", JsonConvert.SerializeObject(command));
            
            var factory = new ConnectionFactory { HostName = "localhost"};
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
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
                basicProperties: null,
                body: body);

            return Accepted();
            
            // generate exceptions
            // log errors
        }
        
        //bind json

        [HttpGet]
        public IActionResult GetApplicationStatus(GetApplicationByRequestIdCommand command)
        {
            _logger.LogInformation("Get application status with request id: {0}", command.RequestId);
            
            //reuse? 
            var factory = new ConnectionFactory { HostName = "localhost"};
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
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
                basicProperties: null,
                body: body);

            return Accepted();
            
            // generate exceptions
            // log errors
        }
        
        [HttpGet]
        public IActionResult GetApplicationStatus(GetApplicationByClientIdCommand command)
        {
            _logger.LogInformation("Get application status with client id {0}, department address {1}", command.ClientId, command.DepartmentAddress);
            
            //reuse? 
            var factory = new ConnectionFactory { HostName = "localhost"};
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
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
                basicProperties: null,
                body: body);

            return Accepted();
            
            // generate exceptions
            // log errors
        }
    }
}