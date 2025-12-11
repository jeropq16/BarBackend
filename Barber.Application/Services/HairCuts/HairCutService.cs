using Barber.Application.DTOs.HairCuts;
using Barber.Domain.Interfaces;

namespace Barber.Application.Services.Services;

public class HairCutService
{
    private readonly IHairCutRepository _serviceRepo;

    public HairCutService(IHairCutRepository serviceRepo)
    {
        _serviceRepo = serviceRepo;
    }

    public async Task<IEnumerable<HairCutResponse>> GetAllAsync()
    {
        var services = await _serviceRepo.GetAllAsync();

        return services.Select(s => new HairCutResponse
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            Price = s.Price,
            DurationMinutes = s.DurationMinutes
        });
    }
}