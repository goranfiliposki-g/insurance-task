using Claims.Application.DTOs;

namespace Claims.Application.Interfaces;

public interface IClaimService
{
    Task<string> CreateAsync(CreateClaimDto dto);
    Task<ClaimDto?> GetByIdAsync(string id);
    Task<IEnumerable<ClaimDto>> GetAllAsync();
    Task DeleteAsync(string id);
}
