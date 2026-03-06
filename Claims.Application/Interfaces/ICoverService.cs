using Claims.Application.DTOs;
using Claims.Domain.Entities;

namespace Claims.Application.Interfaces;

public interface ICoverService
{
    Task<string> CreateAsync(CreateCoverDto dto);
    Task<CoverDto?> GetByIdAsync(string id);
    Task<IEnumerable<CoverDto>> GetAllAsync();
    Task DeleteAsync(string id);
    decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType);
}
