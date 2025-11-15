namespace FleetRent.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; private set; }
        public Guid BikeId { get; private set; }
        public string BikeIdentifier { get; private set; } 
        public int BikeYear { get; private set; }
        public string Message { get; private set; } 
        public DateTime CreatedAt { get; private set; }

        private Notification() { }

        public Notification(Guid bikeId, string identifier, int year, string message)
        {
            Id = Guid.NewGuid();
            BikeId = bikeId;
            BikeIdentifier = identifier;
            BikeYear = year;
            Message = message;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
