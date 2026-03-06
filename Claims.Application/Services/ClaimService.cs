using Claims.Application.Common;
using Claims.Application.DTOs;
using Claims.Application.Interfaces;
using Claims.Domain.Entities;
using FluentValidation;

namespace Claims.Application.Services;

public class ClaimService : IClaimService
{
    private readonly IClaimRepository _repository;
    private readonly ICoverRepository _coverRepository;
    private readonly IAuditService _auditService;
    private readonly IValidator<CreateClaimDto> _validator;

    public ClaimService(
        IClaimRepository repository,
        ICoverRepository coverRepository,
        IAuditService auditService,
        IValidator<CreateClaimDto> validator)
    {
        _repository = repository;
        _coverRepository = coverRepository;
        _auditService = auditService;
        _validator = validator;
    }

    public async Task<string> CreateAsync(CreateClaimDto dto)
    {
        var result = await _validator.ValidateAsync(dto);
        if (!result.IsValid)
        {
            throw new Common.ValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
        }

        var claim = new Claim
        {
            Id = Guid.NewGuid().ToString(),
            CoverId = dto.CoverId,
            Created = dto.Created,
            Name = dto.Name,
            Type = dto.Type,
            DamageCost = dto.DamageCost
        };

        await _repository.AddAsync(claim);
        _ = _auditService.AuditClaimAsync(claim.Id, AuditAction.Create); 
        return claim.Id;
    }

    public async Task<Claim?> GetByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<Claim>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (deleted)
        {
            _ = _auditService.AuditClaimAsync(id, AuditAction.Delete);
        }

        return deleted;
    }
}
