using FleetRent.Application.Common.Validtaions;
using System.ComponentModel.DataAnnotations;

namespace FleetRent.Application.Rentals.Dtos
{ 
    public class CloseRentalDto
    {
        [Required(ErrorMessage = "ReturnDate is required.")]
        [DateTimeValidation(ErrorMessage = "Invalid ReturnDate.")]
        public DateTime ReturnDate { get; set; }
    }
}
