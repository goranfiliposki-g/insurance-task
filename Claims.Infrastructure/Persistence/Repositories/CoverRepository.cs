using Claims.Application.Interfaces;
using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Persistence.Repositories;

public class CoverRepository : ICoverRepository
{
    private readonly ClaimsDbContext _context;

    public CoverRepository(ClaimsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Cover cover)
    {
        _context.Covers.Add(cover);
        await _context.SaveChangesAsync();
    }

    public async Task<Cover?> GetByIdAsync(string id)
        => await _context.Covers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IReadOnlyList<Cover>> GetAllAsync()
        => await _context.Covers.AsNoTracking().ToListAsync();

    public async Task<bool> DeleteAsync(string id)
    {
        var cover = await _context.Covers.FindAsync(id);
        if (cover == null)
        {
            return false;
        }

        _context.Covers.Remove(cover);
        await _context.SaveChangesAsync();
        return true;
    }
}
