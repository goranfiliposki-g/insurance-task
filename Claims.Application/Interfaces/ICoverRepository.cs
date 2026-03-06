using Claims.Domain.Entities;

namespace Claims.Application.Interfaces;

public interface ICoverRepository
{
    Task AddAsync(Cover cover);
    Task<Cover?> GetByIdAsync(string id);
    Task<IEnumerable<Cover>> GetAllAsync();
    Task DeleteAsync(string id);
}
