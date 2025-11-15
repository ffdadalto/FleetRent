using FleetRent.Domain.Enums;
using FleetRent.Domain.Exceptions;
using FleetRent.Domain.ValueObjects;

namespace FleetRent.Domain.Entities
{
    public class Rental
    {
        public Guid Id { get; private set; }
        public Guid DriverId { get; private set; }
        public Guid BikeId { get; private set; }

        public string Identifier { get; private set; }

        public DateTime StartDate { get; private set; }
        public DateTime PlannedEndDate { get; private set; }
        public DateTime? EndDate { get; private set; }

        public RentalPlan Plan { get; private set; } 

        public virtual Driver Driver { get; private set; } 
        public virtual Bike Bike { get; private set; } 

        private Rental() { }

        public Rental(Guid driverId, Guid bikeId, string identifier, DateTime startDate, RentalPlanType planType)
        {
            Id = Guid.NewGuid();
            DriverId = driverId;
            BikeId = bikeId;
            Identifier = identifier;
            Plan = RentalPlan.FromType(planType);
            // Business requirement: rentals start the day after the booking request.
            StartDate = startDate.AddDays(1);
            PlannedEndDate = StartDate.AddDays(Plan.Days);
        }

        public decimal CloseAndCalculateTotal(DateTime returnDate)
        {            
            var returnDateOnly = DateOnly.FromDateTime(returnDate);
            var startDateOnly = DateOnly.FromDateTime(StartDate);
            var plannedEndDateOnly = DateOnly.FromDateTime(PlannedEndDate);

            if (returnDateOnly < startDateOnly)
                throw DomainException.From(DomainErrors.Rental.ReturnBeforeStart);

            EndDate ??= returnDate;

            var baseCost = Plan.Days * Plan.DailyRate;

            if (returnDateOnly == plannedEndDateOnly)
                return baseCost;

            if (returnDateOnly < plannedEndDateOnly)
            {
                var usedDays = (returnDateOnly.DayNumber - startDateOnly.DayNumber) + 1;
                var unusedDays = Plan.Days - usedDays;

                var usedCost = usedDays * Plan.DailyRate;
                var fineBase = unusedDays * Plan.DailyRate;
                var fine = fineBase * Plan.EarlyReturnFinePercentage;

                return usedCost + fine;
            }
            else
            {
                var extraDays = returnDateOnly.DayNumber - plannedEndDateOnly.DayNumber;
                var extraCost = extraDays * 50m;
                return baseCost + extraCost;
            }
        }

    }
}
