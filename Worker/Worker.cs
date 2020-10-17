using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IConnection _connection;
        private static IModel _channel;
        
        private const string AddQueueName = "AddApplicationQueue";
        private const string GetByRequestIdQueueName = "GetByRequestQueue";
        private const string GetByClientIdQueueName = "GetByClientQueue";

        public Worker(ILogger<Worker> logger, IRepository repository, IConnection connection)
        {
            _logger = logger;
            _repository = repository;
            _connection = connection;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _channel = _connection.CreateModel();
            
            SubscribeToQueue(AddQueueName, AddApplication);
            SubscribeToQueue(GetByClientIdQueueName, GetByClientId);
            SubscribeToQueue(GetByRequestIdQueueName, GetByRequestId);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Worker running at: {DateTimeOffset.Now}, awaiting requests");
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void AddApplication(object sender, BasicDeliverEventArgs args)
        {
            var response = string.Empty;
            try
            {
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"Consumer received message: {message}");

                var addCommand = JsonConvert.DeserializeObject<AddApplicationMqCommand>(message);
                response = _repository.AddApplication(addCommand).ToString();
                _logger.LogInformation($"Response from database: {response}");
                
                var responseBytes = Encoding.UTF8.GetBytes(response);
                Reply(responseBytes, args);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
        
        private void GetByClientId(object sender, BasicDeliverEventArgs args)
        {
            var response = Enumerable.Empty<ApplicationModel>();
            try
            {
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"Consumer received message: {message}");

                var getCommand = JsonConvert.DeserializeObject<GetApplicationByClientIdMqCommand>(message);
                response = _repository.GetApplicationsByClientId(getCommand).ToList();
                _logger.LogInformation($"Response from database: {response}");
                
                var responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                Reply(responseBytes, args);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
        
        private void GetByRequestId(object sender, BasicDeliverEventArgs args)
        {
            var response = Enumerable.Empty<ApplicationModel>();
            try
            {
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"Consumer received message: {message}");

                var getCommand = JsonConvert.DeserializeObject<GetApplicationByRequestIdMqCommand>(message);
                response = _repository.GetApplicationsByRequestId(getCommand).ToList();
                _logger.LogInformation($"Response from database: {response}");

                var responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
                Reply(responseBytes, args);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
        
        private void SubscribeToQueue(string queueName, EventHandler<BasicDeliverEventArgs> callback)
        {
            _channel.QueueDeclare(queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += callback;
            _channel.BasicConsume(queue: queueName,
                autoAck: false,
                consumer: consumer);
        }

        private void Reply(byte[] responseBytes, BasicDeliverEventArgs args)
        {
            var props = args.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;
            
            _channel.BasicPublish(exchange: "", 
                routingKey: props.ReplyTo,
                basicProperties: replyProps, 
                body: responseBytes);
            _channel.BasicAck(deliveryTag: args.DeliveryTag,
                multiple: false);

            _logger.LogInformation($"Sent response: {Encoding.UTF8.GetString(responseBytes)}");
        }
    }
}