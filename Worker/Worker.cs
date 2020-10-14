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
        private readonly IRepository _repository;

        public Worker(ILogger<Worker> logger, IRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory {HostName = "localhost"};
            using var connection = factory.CreateConnection();

            var addCallback = GetAddCommand(new AddApplicationMqCommand());
            ReceiveMessage(connection, "AddApplicationQueue", GetAddCommand(new AddApplicationMqCommand()));
            var addCommand = addCallback.Item2;
            var id = _repository.AddApplication(addCommand);

            var getByRequestCallback = GetGetByRequestCommand(new GetApplicationByRequestIdMqCommand());
            ReceiveMessage(connection, "GetByRequestQueue", getByRequestCallback);
            var getByRequestCommand = getByRequestCallback.Item2;
            var applications = _repository.GetApplicationsByRequestId(getByRequestCommand);

            var getByClientCallback = GetGetByClientCommand(new GetApplicationByClientIdMqCommand());
            ReceiveMessage(connection, "GetByClientQueue", getByClientCallback);
            var getByClientCommand = getByClientCallback.Item2;
            applications = _repository.GetApplicationsByClientId(getByClientCommand);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
        
        private static void ReceiveMessage(IConnection connection, string queryName, (EventHandler<BasicDeliverEventArgs>, ICommand) callback)
        {
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: queryName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += callback.Item1;
            channel.BasicConsume(queue: queryName,
                autoAck: true,
                consumer: consumer);
        }

        private static (EventHandler<BasicDeliverEventArgs>, AddApplicationMqCommand) GetAddCommand(AddApplicationMqCommand addCommand)
        {
            return ((model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                addCommand = JsonConvert.DeserializeObject<AddApplicationMqCommand>(message);
            }, addCommand);
        }
        
        private static (EventHandler<BasicDeliverEventArgs>, GetApplicationByRequestIdMqCommand) GetGetByRequestCommand(GetApplicationByRequestIdMqCommand getByRequestCommand)
        {
            return ((model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                getByRequestCommand = JsonConvert.DeserializeObject<GetApplicationByRequestIdMqCommand>(message);
            }, getByRequestCommand);
        }

        private static (EventHandler<BasicDeliverEventArgs>, GetApplicationByClientIdMqCommand) GetGetByClientCommand(GetApplicationByClientIdMqCommand getByClientCommand)
        {
            return ((model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                getByClientCommand = JsonConvert.DeserializeObject<GetApplicationByClientIdMqCommand>(message);
            }, getByClientCommand);
        }
    }
}