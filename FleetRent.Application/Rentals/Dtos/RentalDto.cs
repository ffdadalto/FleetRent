using FleetRent.Domain.Entities;
using FleetRent.Domain.Enums;

namespace FleetRent.Application.Rentals.Dtos
{
    public class RentalDto
    {
        public Guid Id { get; set; }
        public string Identifier { get; set; }
        public decimal DailyRate { get; set; }
        public Guid DriverId { get; set; }
        public Guid BikeId { get; set; }        
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public decimal? TotalCost { get; set; }
        public RentalPlanType PlanType { get; set; }

        public static RentalDto FromEntity(Rental r) => new()
        {
            Id = r.Id,
            Identifier = r.Identifier,
            DailyRate = r.Plan.DailyRate,
            DriverId = r.DriverId,
            BikeId = r.BikeId,            
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            PlannedEndDate = r.PlannedEndDate,  
            PlanType = r.Plan.Type,

        };
    }
}
