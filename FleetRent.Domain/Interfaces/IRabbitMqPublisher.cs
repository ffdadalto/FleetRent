namespace FleetRent.Domain.Interfaces
{
    public interface IRabbitMqPublisher
    {
        Task PublishAsync(string routingKey, string message, CancellationToken ct = default);
    }
}
