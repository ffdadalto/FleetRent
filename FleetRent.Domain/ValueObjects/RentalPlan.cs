using FleetRent.Domain.Enums;

namespace FleetRent.Domain.ValueObjects
{
    public class RentalPlan
    {
        public RentalPlanType Type { get; private set; }
        public int Days { get; private set; }
        public decimal DailyRate { get; private set; }
        public decimal EarlyReturnFinePercentage { get; private set; }

        private RentalPlan() { }

        private RentalPlan(RentalPlanType type, int days, decimal dailyRate, decimal finePercentage)
        {
            Type = type;
            Days = days;
            DailyRate = dailyRate;
            EarlyReturnFinePercentage = finePercentage;
        }

        public static RentalPlan FromType(RentalPlanType type)
        {
            return type switch
            {
                RentalPlanType.Days7 => new RentalPlan(type, 7, 30m, 0.20m),
                RentalPlanType.Days15 => new RentalPlan(type, 15, 28m, 0.40m),
                RentalPlanType.Days30 => new RentalPlan(type, 30, 22m, 0m),
                RentalPlanType.Days45 => new RentalPlan(type, 45, 20m, 0m),
                RentalPlanType.Days50 => new RentalPlan(type, 50, 18m, 0m),
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }
    }
}
