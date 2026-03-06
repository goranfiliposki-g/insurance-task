using Claims.Domain.Entities;

namespace Claims.Application.Interfaces
{
    public interface IClaimRepository
    {
    Task AddAsync(Claim claim);
    Task<IReadOnlyList<Claim>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
    Task<Claim?> GetByIdAsync(string id);
    }
}
