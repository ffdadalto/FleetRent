using FleetRent.Domain.Exceptions;

namespace FleetRent.Domain.Entities
{
    public class Bike
    {
        public Guid Id { get; private set; }
        public string Identifier { get; private set; }
        public int Year { get; private set; }
        public string Model { get; private set; }
        public string Plate { get; private set; }

        private readonly List<Rental> _rentals = [];
        public IReadOnlyCollection<Rental> Rentals => _rentals.AsReadOnly();

        private Bike() { }

        public Bike(string identifier, int year, string model, string plate)
        {
            Id = Guid.NewGuid();
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Year = year;
            Model = model ?? throw new ArgumentNullException(nameof(model));
            ChangePlate(plate);
        }

        public void ChangePlate(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate))
                throw DomainException.From(DomainErrors.Bike.PlateRequired);

            Plate = plate.Trim().ToUpperInvariant();
        }

        public bool HasRentals => _rentals.Count != 0;
        public bool CanDelete => !HasRentals;
    }
}
