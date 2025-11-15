using System.ComponentModel.DataAnnotations;

namespace FleetRent.Application.Bikes.Dtos
{
    public class CreateBikeDto
    {
        [Required(ErrorMessage = "Identifier is required")]
        public string Identifier { get; set; }

        [Required(ErrorMessage = "Year is required")]
        [Range(1950, 2100, ErrorMessage = "Year is required")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Model is required")]
        public string Model { get; set; }

        [Required(ErrorMessage = "Plate is required")]
        public string Plate { get; set; }
    }
}
