using Claims.Application.DTOs;
using Claims.Domain.Entities;
using Claims.Domain.Enums;

namespace Claims.Application.Interfaces;

public interface ICoverService
{
    Task<string> CreateAsync(CreateCoverDto dto);
    Task<Cover?> GetByIdAsync(string id);
    Task<IReadOnlyList<Cover>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
    decimal ComputePremium(DateOnly startDate, DateOnly endDate, string coverType);
}
