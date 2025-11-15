using FleetRent.Application.Common.Validtaions;
using FleetRent.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FleetRent.Application.Drivers.Dtos
{
    public class CreateDriverDto
    {

        [Required(ErrorMessage = "Identifier is required")]
        public string Identifier { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Cnpj is required")]
        public string Cnpj { get; set; }

        [Required(ErrorMessage = "BirthDate is required.")]
        [DateValidation(ErrorMessage = "Invalid BirthDate.")]
        public DateOnly BirthDate { get; set; }

        [Required(ErrorMessage = "LicenseNumber is required")]
        public string LicenseNumber { get; set; }

        [Required] 
        public LicenseType LicenseType { get; set; }

        [Required(ErrorMessage = "LicenseImageBase64 is required")]
        public string LicenseImageBase64 { get; set; }
    }
}