using Barber.Domain.Models;

namespace Barber.Domain.Interfaces;

public interface IHairCutRepository
{
    Task<IEnumerable<HairCut>> GetAllAsync();
    Task<HairCut?> GetByIdAsync(int id);
}