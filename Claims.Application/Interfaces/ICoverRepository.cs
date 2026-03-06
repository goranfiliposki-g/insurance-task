using Claims.Domain.Entities;

namespace Claims.Application.Interfaces;

public interface ICoverRepository
{
    Task AddAsync(Cover cover);
    Task<Cover?> GetByIdAsync(string id);
    Task<IReadOnlyList<Cover>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}
