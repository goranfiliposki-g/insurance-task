using Claims.Application.DTOs;
using Claims.Domain.Entities;

namespace Claims.Application.Interfaces;

public interface IClaimService
{
    Task<string> CreateAsync(CreateClaimDto dto);
    Task<Claim?> GetByIdAsync(string id);
    Task<IReadOnlyList<Claim>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}
