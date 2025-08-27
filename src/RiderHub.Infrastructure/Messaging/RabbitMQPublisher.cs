using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RiderHub.Application.Interfaces;
using System.Text;

namespace RiderHub.Infrastructure.Messaging
{
    public class RabbitMQPublisher : IMessagePublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName;

        public RabbitMQPublisher(IConfiguration configuration)
        {
            var factory = new ConnectionFactory() { HostName = configuration["RabbitMQ:HostName"] };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _queueName = configuration["RabbitMQ:QueueName"];
            _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public void Publish(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);
        }
    }
}
