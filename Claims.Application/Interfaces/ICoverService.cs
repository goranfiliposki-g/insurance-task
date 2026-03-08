using Claims.Application.DTOs;
using Claims.Domain.Entities;

namespace Claims.Application.Interfaces;

public interface ICoverService
{
    Task<string> CreateAsync(CreateCoverDto dto);
    Task<Cover?> GetByIdAsync(string id);
    Task<IReadOnlyList<Cover>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
    Task<decimal> ComputePremiumAsync(DateOnly startDate, DateOnly endDate, string coverType, CancellationToken cancellationToken = default);
}
