namespace FleetRent.Infrastructure.Messaging
{
    public record BikeCreatedMessage(Guid BikeId, string Identifier, int Year);
}  