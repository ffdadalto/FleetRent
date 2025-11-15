using FleetRent.Domain.Entities;

namespace FleetRent.Application.Bikes.Dtos
{
    public class BikeDto
    {
        public Guid Id { get; set; }
        public string Identifier { get; set; } = default!;
        public int Year { get; set; }
        public string Model { get; set; } = default!;
        public string Plate { get; set; } = default!;

        public static BikeDto FromEntity(Bike bike) =>
            new()
            {
                Id = bike.Id,
                Identifier = bike.Identifier,
                Year = bike.Year,
                Model = bike.Model,
                Plate = bike.Plate
            };
    }
}
