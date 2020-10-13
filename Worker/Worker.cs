using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory {HostName = "localhost"};
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
            channel.QueueDeclare(queue: "AddApplicationQueue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var command = JsonConvert.DeserializeObject<AddApplicationMqCommand>(message);
            };
            channel.BasicConsume(queue: "AddApplicationQueue",
                autoAck: true,
                consumer: consumer);

            
            
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}