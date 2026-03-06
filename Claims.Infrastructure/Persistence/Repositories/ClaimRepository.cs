using Claims.Application.Interfaces;
using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Persistence.Repositories;

public class ClaimRepository : IClaimRepository
{
    private readonly ClaimsDbContext _context;

    public ClaimRepository(ClaimsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Claim claim)
    {
        _context.Claims.Add(claim);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Claim>> GetAllAsync()
        => await _context.Claims.AsNoTracking().ToListAsync();

    public async Task<Claim?> GetByIdAsync(string id)
        => await _context.Claims.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<bool> DeleteAsync(string id)
    {
        var claim = await _context.Claims.FindAsync(id);
        if (claim == null) return false;

        _context.Claims.Remove(claim);
        await _context.SaveChangesAsync();
        return true;
    }
}
