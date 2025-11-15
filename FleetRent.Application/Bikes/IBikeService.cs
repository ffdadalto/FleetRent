using FleetRent.Application.Bikes.Dtos;

namespace FleetRent.Application.Bikes
{
    public interface IBikeService
    {
        Task<BikeDto> CreateAsync(CreateBikeDto dto);
        Task<IReadOnlyCollection<BikeDto>> GetAsync(string plate);
        Task<BikeDto> GetByIdAsync(Guid id);
        Task<BikeDto> UpdatePlateAsync(Guid id, UpdateBikePlateDto dto);
        Task DeleteAsync(Guid id);
    }   
}
