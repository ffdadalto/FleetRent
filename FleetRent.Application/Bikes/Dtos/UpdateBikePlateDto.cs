using System.ComponentModel.DataAnnotations;

namespace FleetRent.Application.Bikes.Dtos
{
    public class UpdateBikePlateDto
    {
        [Required(ErrorMessage = "Plate is required")]
        public string Plate { get; set; }
    }
}
