using Claims.Application.Common;
using Claims.Application.DTOs;
using Claims.Application.Interfaces;
using Claims.Domain.Entities;
using Claims.Domain.Enums;
using FluentValidation;

namespace Claims.Application.Services;

public class CoverService : ICoverService
{
    private readonly ICoverRepository _repository;
    private readonly IAuditService _auditService;
    private readonly IPremiumCalculator _premiumCalculator;
    private readonly IPremiumPolicy _defaultPolicy;
    private readonly IValidator<CreateCoverDto> _validator;

    public CoverService(
        ICoverRepository repository,
        IAuditService auditService,
        IPremiumCalculator premiumCalculator,
        IPremiumPolicy defaultPolicy,
        IValidator<CreateCoverDto> validator)
    {
        _repository = repository;
        _auditService = auditService;
        _premiumCalculator = premiumCalculator;
        _defaultPolicy = defaultPolicy;
        _validator = validator;
    }

    public async Task<string> CreateAsync(CreateCoverDto dto)
    {
        var result = await _validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            throw new Common.ValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var premium = await _premiumCalculator.ComputeAsync(dto.StartDate, dto.EndDate, dto.Type, _defaultPolicy);
        var cover = new Cover
        {
            Id = Guid.NewGuid().ToString(),
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Type = dto.Type,
            Premium = premium
        };
        await _repository.AddAsync(cover);
        _ = _auditService.AuditCoverAsync(cover.Id, AuditAction.Create);
        return cover.Id;
    }

    public async Task<Cover?> GetByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<Cover>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (deleted)
        {
            _ = _auditService.AuditCoverAsync(id, AuditAction.Delete);
        }

        return deleted;
    }

    public async Task<decimal> ComputePremiumAsync(DateOnly startDate, DateOnly endDate, string coverType, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<CoverType>(coverType, ignoreCase: true, out var type))
        {
            throw new Common.ValidationException(new[] { $"Unknown cover type: '{coverType}'. Valid values: {string.Join(", ", Enum.GetNames<CoverType>())}" });
        }

        return await _premiumCalculator.ComputeAsync(startDate, endDate, type, _defaultPolicy);
    }
}
