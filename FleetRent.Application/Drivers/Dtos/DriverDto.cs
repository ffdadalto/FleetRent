using FleetRent.Domain.Entities;
using FleetRent.Domain.Enums;

namespace FleetRent.Application.Drivers.Dtos
{
    public class DriverDto
    {
        public Guid Id { get; set; }
        public string Identifier { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Cnpj { get; set; } = default!;
        public DateOnly BirthDate { get; set; }
        public string LicenseNumber { get; set; } = default!;
        public LicenseType LicenseType { get; set; }
        public string LicenseImagePath { get; private set; }
        public string LicenseImageBase64 { get; private set; }

        public static DriverDto FromEntity(Driver drive)
        {
            var dto = new DriverDto
            {
                Id = drive.Id,
                Identifier = drive.Identifier,
                Name = drive.Name,
                Cnpj = drive.Cnpj,
                BirthDate = drive.BirthDate,
                LicenseNumber = drive.LicenseNumber,
                LicenseType = drive.LicenseType,
                LicenseImagePath = drive.LicenseImagePath
            };

            if (!string.IsNullOrWhiteSpace(dto.LicenseImagePath) && File.Exists(dto.LicenseImagePath))
            {
                try
                {
                    var bytes = File.ReadAllBytes(dto.LicenseImagePath);
                    dto.LicenseImageBase64 = Convert.ToBase64String(bytes);
                }
                catch
                {
                    dto.LicenseImageBase64 = null;
                }
            }

            return dto;
        }
    }
}
