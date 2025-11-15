using System.ComponentModel.DataAnnotations;

namespace FleetRent.Application.Drivers.Dtos
{
    public class UpdateDriverLicenseImageDto
    {
        [Required(ErrorMessage = "ImageBase64 is required")]
        public string ImageBase64 { get; set; }
    }
}
