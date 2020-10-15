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
        private static IModel channel;

        public Worker(ILogger<Worker> logger, IRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connection = GetConnection();
            channel = connection.CreateModel();
            
            ReceiveMessage("AddApplicationQueue", AddApplication);
            ReceiveMessage("GetByClientQueue", GetByClientId);
            ReceiveMessage("GetByRequestQueue", GetByRequestId);
            
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
        
        private static void ReceiveMessage(string queueName, EventHandler<BasicDeliverEventArgs> callback)
        {
            channel.QueueDeclare(queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += callback;
            channel.BasicConsume(queue: queueName,
                autoAck: false,
                consumer: consumer);
        }

        private static void Reply(byte[] responseBytes, BasicDeliverEventArgs args)
        {
            var props = args.BasicProperties;
            var replyProps = channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;
            
            channel.BasicPublish(exchange: "", 
                routingKey: props.ReplyTo,
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