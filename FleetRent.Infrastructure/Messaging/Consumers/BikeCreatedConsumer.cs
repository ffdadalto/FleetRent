using FleetRent.Domain.Entities;
using FleetRent.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace FleetRent.Infrastructure.Messaging.Consumers
{
    public class BikeCreatedConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BikeCreatedConsumer> _logger;
        private readonly ConnectionFactory _factory;
        private readonly string _exchange;
        private readonly string _queueName;

        public BikeCreatedConsumer(IConfiguration cfg, IServiceProvider serviceProvider, ILogger<BikeCreatedConsumer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            var host = cfg["RabbitMq:HostName"] ?? "rabbit";
            var user = cfg["RabbitMq:UserName"] ?? "guest";
            var pass = cfg["RabbitMq:Password"] ?? "guest";

            _exchange = cfg["RabbitMq:Exchange"] ?? "fleet.exchange";
            _queueName = cfg["RabbitMq:BikeCreatedQueue"] ?? "fleet.bike-created.notifications";

            _factory = new ConnectionFactory
            {
                HostName = host,
                UserName = user,
                Password = pass,
                DispatchConsumersAsync = true
            };
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connection = _factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true);
            channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(_queueName, _exchange, routingKey: "bike.created");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += async (_, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                try
                {
                    var msg = JsonSerializer.Deserialize<BikeCreatedMessage>(json);
                    if (msg is not null && msg.Year == 2024)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var notification = new Notification(
                            msg.BikeId,
                            msg.Identifier,
                            msg.Year,
                            $"Bike {msg.Identifier} from {msg.Year} created.");

                        await repo.AddAsync(notification, stoppingToken);
                        await uow.SaveChangesAsync(stoppingToken);
                    }

                    channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing bike created message: {Json}", json);
                    channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            channel.BasicConsume(_queueName, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }
    }
}
