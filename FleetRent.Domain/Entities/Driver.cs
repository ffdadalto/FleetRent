using FleetRent.Domain.Enums;

namespace FleetRent.Domain.Entities
{
    public class Driver
    {
        public Guid Id { get; private set; }
        public string Identifier { get; private set; } 
        public string Name { get; private set; } 
        public string Cnpj { get; private set; } 
        public DateOnly BirthDate { get; private set; }
        public string LicenseNumber { get; private set; } 
        public LicenseType LicenseType { get; private set; }
        public string LicenseImagePath { get; private set; }

        private readonly List<Rental> _rentals = new();
        public IReadOnlyCollection<Rental> Rentals => _rentals.AsReadOnly();

        private Driver() { }

        public Driver(string identifier, string name, string cnpj, DateOnly birthDate, string licenseNumber, LicenseType licenseType)
        {
            Id = Guid.NewGuid();
            Identifier = identifier;
            Name = name;
            Cnpj = cnpj;
            BirthDate = birthDate;
            LicenseNumber = licenseNumber;
            LicenseType = licenseType;
        }

        public bool IsCategoryAEnabled() =>
            LicenseType == LicenseType.A || LicenseType == LicenseType.AB;

        public void UpdateLicenseImage(string path)
        {
            LicenseImagePath = path;
        }
    }
}
