using Claims.Application.Common;
using Claims.Application.DTOs;
using Claims.Application.Interfaces;
using Claims.Domain.Entities;

namespace Claims.Application.Services;

public class CoverService : ICoverService
{
    private const int MaxInsuranceDays = 365;

    private readonly ICoverRepository _repository;
    private readonly IAuditService _auditService;
    private readonly IPremiumCalculator _premiumCalculator;

    public CoverService(ICoverRepository repository, IAuditService auditService, IPremiumCalculator premiumCalculator)
    {
        _repository = repository;
        _auditService = auditService;
        _premiumCalculator = premiumCalculator;
    }

    public async Task<string> CreateAsync(CreateCoverDto dto)
    {
        ValidateCreate(dto);

        var premium = _premiumCalculator.Compute(dto.StartDate, dto.EndDate, dto.Type);
        var cover = new Cover
        {
            Id = Guid.NewGuid().ToString(),
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Type = dto.Type,
            Premium = premium
        };
        await _repository.AddAsync(cover);
        _ = _auditService.AuditCoverAsync(cover.Id, "POST");
        return cover.Id;
    }

    public async Task<CoverDto?> GetByIdAsync(string id)
    {
        var cover = await _repository.GetByIdAsync(id);
        return cover == null ? null : ToDto(cover);
    }

    public async Task<IEnumerable<CoverDto>> GetAllAsync()
    {
        var covers = await _repository.GetAllAsync();
        return covers.Select(ToDto);
    }

    public async Task DeleteAsync(string id)
    {
        _ = _auditService.AuditCoverAsync(id, "DELETE");
        await _repository.DeleteAsync(id);
    }

    public decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType)
        => _premiumCalculator.Compute(startDate, endDate, coverType);

    private static void ValidateCreate(CreateCoverDto dto)
    {
        var errors = new List<string>();

        if (dto.StartDate.Date < DateTime.UtcNow.Date)
        {
            errors.Add("StartDate cannot be in the past.");
        }

        var periodDays = (dto.EndDate - dto.StartDate).TotalDays;
        if (periodDays > MaxInsuranceDays)
        {
            errors.Add("Total insurance period cannot exceed 1 year.");
        }

        if (dto.EndDate < dto.StartDate)
        {
            errors.Add("EndDate must be after StartDate.");
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private static CoverDto ToDto(Cover c) =>
        new(c.Id, c.StartDate, c.EndDate, c.Type, c.Premium);
}
