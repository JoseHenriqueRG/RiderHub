using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RiderHub.Domain.Entities;
using RiderHub.Domain.Interfaces;
using System.Text;
using System.Text.Json;

namespace RiderHub.Infrastructure.Messaging
{
    public class RabbitMQConsumer : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly string _queueName;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ConnectionFactory _factory;

        public RabbitMQConsumer(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _factory = new ConnectionFactory() { HostName = configuration["RabbitMQ:HostName"] };
            _queueName = configuration["RabbitMQ:QueueName"];
            _serviceScopeFactory = serviceScopeFactory;
            
            ConnectToRabbitMQ();
        }

        private void ConnectToRabbitMQ()
        {
            int retryCount = 0;
            const int maxRetries = 10;
            const int delayMilliseconds = 5000; // 5 seconds

            while (true)
            {
                try
                {
                    _connection = _factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                    Console.WriteLine("Connected to RabbitMQ successfully.");
                    break; // Exit loop on successful connection
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Console.WriteLine($"Failed to connect to RabbitMQ. Retrying ({retryCount}/{maxRetries})... Error: {ex.Message}");
                    if (retryCount >= maxRetries)
                    {
                        Console.WriteLine("Max retries reached. Could not connect to RabbitMQ.");
                        throw; // Re-throw if max retries reached
                    }
                    Task.Delay(delayMilliseconds).Wait(); // Wait before retrying
                }
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var motorcycle = JsonSerializer.Deserialize<Motorcycle>(message);

                if (motorcycle?.Year == 2024)
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var notificationRepository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                        var notification = new Notification
                        {
                            Message = $"A new motorcycle from 2024 has been registered: {motorcycle.Model} - {motorcycle.LicensePlate}",
                            Timestamp = DateTime.UtcNow
                        };
                        await notificationRepository.AddAsync(notification);
                    }
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
