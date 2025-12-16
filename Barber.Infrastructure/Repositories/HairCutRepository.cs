using Barber.Domain.Interfaces;
using Barber.Domain.Models;
using Barber.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Barber.Infrastructure.Repositories;

public class HairCutRepository : IHairCutRepository
{
    private readonly AppDbContext _context;

    public HairCutRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HairCut>> GetAllAsync()
    {
        return await _context.HairCuts
            .Where(h => h.IsActive)
            .ToListAsync();
    }

    public async Task<HairCut?> GetByIdAsync(int id)
    { 
        return await _context.HairCuts.FirstOrDefaultAsync(h => h.Id == id);
    }
}