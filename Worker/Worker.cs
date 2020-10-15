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
            var connection = GetConnection();
            
            ReceiveMessage(connection, "AddApplicationQueue", AddApplication);
            ReceiveMessage(connection, "GetByClientQueue", GetByClientId);
            ReceiveMessage(connection, "GetByRequestQueue", GetByRequestId);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void AddApplication(object sender, BasicDeliverEventArgs args)
        {
            var body = args.Body.ToArray();
            
            var message = Encoding.UTF8.GetString(body);
            var addCommand = JsonConvert.DeserializeObject<AddApplicationMqCommand>(message);
            var response = _repository.AddApplication(addCommand);
            
            var responseBytes = Encoding.UTF8.GetBytes(response.ToString());
            Reply(responseBytes, args);
        }
        
        private void GetByClientId(object sender, BasicDeliverEventArgs args)
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var getCommand = JsonConvert.DeserializeObject<GetApplicationByClientIdMqCommand>(message);
            var response = _repository.GetApplicationsByClientId(getCommand);

            var responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
            Reply(responseBytes, args);
        }
        
        private void GetByRequestId(object sender, BasicDeliverEventArgs args)
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var getCommand = JsonConvert.DeserializeObject<GetApplicationByRequestIdMqCommand>(message);
            var response = _repository.GetApplicationsByRequestId(getCommand);
            
            var responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
            Reply(responseBytes, args);
        }
        
        private static void ReceiveMessage(IConnection connection, string queryName, EventHandler<BasicDeliverEventArgs> callback)
        {
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: queryName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += callback;
            channel.BasicConsume(queue: queryName,
                autoAck: false,
                consumer: consumer);
            
            
        }

        //private byte[] responseBytes;
        private static void Reply(byte[] responseBytes, BasicDeliverEventArgs args)
        {
            var channel = GetConnection().CreateModel();

            var props = args.BasicProperties;
            var replyProps = channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;
            
            channel.BasicPublish(exchange: "", 
                routingKey: "amq.rabbitmq.reply-to",
                basicProperties: replyProps, 
                body: responseBytes);
            channel.BasicAck(deliveryTag: args.DeliveryTag,
                multiple: false);
        }

        private static IConnection GetConnection()
        {
            var factory = new ConnectionFactory {HostName = "localhost"};
            return factory.CreateConnection();
        }
    }
}