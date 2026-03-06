using Claims.Domain.Entities;

namespace Claims.Application.Interfaces
{
    public interface IClaimRepository
    {
        Task AddAsync(Claim claim);
        Task<IEnumerable<Claim>> GetAllAsync();
        Task DeleteAsync(string id);
        Task<Claim?> GetByIdAsync(string id);
    }
}
