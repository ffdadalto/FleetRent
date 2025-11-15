using FleetRent.Application.Common.Validtaions;
using FleetRent.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FleetRent.Application.Rentals.Dtos
{
    public class CreateRentalDto
    {
        [Required(ErrorMessage = "DriverId is required")]
        public Guid DriverId { get; set; }

        [Required(ErrorMessage = "BikeId is required")]
        public Guid BikeId { get; set; }

        [Required(ErrorMessage = "StartDate is required")]
        [DateTimeValidation(ErrorMessage = "Invalid BirthDate.")]
        public DateTime StartDate { get; set; }
                
        [DateTimeValidation(ErrorMessage = "Invalid BirthDate.")]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "PlannedEndDate is required")]
        [DateTimeValidation(ErrorMessage = "Invalid PlannedEndDate.")]
        public DateTime PlannedEndDate { get; set; }

        [Required(ErrorMessage = "PlanType is required")]
        public RentalPlanType PlanType { get; set; }
    }
}
