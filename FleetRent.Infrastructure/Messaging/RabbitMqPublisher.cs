using FleetRent.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace FleetRent.Infrastructure.Messaging
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly ConnectionFactory _factory;
        private readonly string _exchange;
        private readonly ILogger<RabbitMqPublisher> _logger;

        public RabbitMqPublisher(IConfiguration cfg, ILogger<RabbitMqPublisher> logger)
        {
            _logger = logger;
            var host = cfg["RabbitMq:HostName"] ?? "rabbit";
            var user = cfg["RabbitMq:UserName"] ?? "guest";
            var pass = cfg["RabbitMq:Password"] ?? "guest";

            _exchange = cfg["RabbitMq:Exchange"] ?? "fleet.exchange";

            _factory = new ConnectionFactory
            {
                HostName = host,
                UserName = user,
                Password = pass
            };

            _logger.LogInformation("RabbitMQ publisher configured for host {Host} and exchange {Exchange}.", host, _exchange);
        }

        public Task PublishAsync(string routingKey, string message, CancellationToken ct = default)
        {
            _logger.LogInformation("Publishing message to exchange {Exchange} with routing key {RoutingKey}.", _exchange, routingKey);

            try
            {
                ct.ThrowIfCancellationRequested();

                using var conn = _factory.CreateConnection();
                using var channel = conn.CreateModel();

                channel.ExchangeDeclare(_exchange, ExchangeType.Topic, durable: true);

                var body = Encoding.UTF8.GetBytes(message);
                var props = channel.CreateBasicProperties();
                props.Persistent = true;

                channel.BasicPublish(
                    exchange: _exchange,
                    routingKey: routingKey,
                    basicProperties: props,
                    body: body);

                _logger.LogInformation("Message published to exchange {Exchange} with routing key {RoutingKey}.", _exchange, routingKey);

                return Task.CompletedTask;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Publishing to exchange {Exchange} with routing key {RoutingKey} was cancelled.", _exchange, routingKey);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message to exchange {Exchange} with routing key {RoutingKey}.", _exchange, routingKey);
                throw;
            }
        }
    }
}
