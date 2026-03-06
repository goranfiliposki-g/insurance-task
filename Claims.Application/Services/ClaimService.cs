using Claims.Application.Common;
using Claims.Application.DTOs;
using Claims.Application.Interfaces;
using Claims.Domain.Entities;

namespace Claims.Application.Services;

public class ClaimService : IClaimService
{
    private readonly IClaimRepository _repository;
    private readonly ICoverRepository _coverRepository;
    private readonly IAuditService _auditService;

    public ClaimService(IClaimRepository repository, ICoverRepository coverRepository, IAuditService auditService)
    {
        _repository = repository;
        _coverRepository = coverRepository;
        _auditService = auditService;
    }

    public async Task<string> CreateAsync(CreateClaimDto dto)
    {

        await ValidateCreate(dto, _coverRepository);

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
        _ = _auditService.AuditClaimAsync(claim.Id, "POST"); 
        return claim.Id;
    }

    public async Task<ClaimDto?> GetByIdAsync(string id)
    {
        var claim = await _repository.GetByIdAsync(id);
        return claim == null ? null : ToDto(claim);
    }

    public async Task<IEnumerable<ClaimDto>> GetAllAsync()
    {
        var claims = await _repository.GetAllAsync();
        return claims.Select(ToDto);
    }

    public async Task DeleteAsync(string id)
    {
        _ = _auditService.AuditClaimAsync(id, "DELETE");
        await _repository.DeleteAsync(id);
    }

    private static ClaimDto ToDto(Claim c) =>
        new(c.Id, c.CoverId, c.Created, c.Name, c.Type, c.DamageCost);

    private async Task ValidateCreate(CreateClaimDto dto, ICoverRepository coverRepository)
    {
        var errors = new List<string>();

        if (dto.DamageCost > 100_000m)
        {
            errors.Add("DamageCost cannot exceed 100,000.");
        }

        var cover = await coverRepository.GetByIdAsync(dto.CoverId);
        if (cover == null)
        {
            errors.Add("Cover not found.");
        }
        else
        {
            var createdDate = dto.Created.Date;
            if (createdDate < cover.StartDate.Date || createdDate > cover.EndDate.Date)
            {
                errors.Add("Created date must be within the related Cover's period.");
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

    }
}
