using Claims.Application.Common;
using Claims.Application.DTOs;
using Claims.Application.Interfaces;
using FluentValidation;

namespace Claims.Application.Validators;

public class CreateClaimDtoValidator : AbstractValidator<CreateClaimDto>
{
    public CreateClaimDtoValidator(ICoverRepository coverRepository)
    {
        RuleFor(x => x.DamageCost)
            .LessThanOrEqualTo(ClaimValidationConstants.MaxDamageCost)
            .WithMessage("DamageCost cannot exceed 100,000.");

        RuleFor(x => x.CoverId)
            .MustAsync(async (id, _) => await coverRepository.GetByIdAsync(id) != null)
            .WithMessage("Cover not found.");

        RuleFor(x => x)
            .MustAsync(CreatedBeWithinCoverPeriodAsync)
            .WithMessage("Created date must be within the related Cover's period.")
            .When(x => !string.IsNullOrEmpty(x.CoverId));

        _coverRepository = coverRepository;
    }

    private readonly ICoverRepository _coverRepository;

    private async Task<bool> CreatedBeWithinCoverPeriodAsync(CreateClaimDto dto, CancellationToken ct)
    {
        var cover = await _coverRepository.GetByIdAsync(dto.CoverId);
        if (cover == null)
        {
            return true;
        }

        var createdDate = DateOnly.FromDateTime(dto.Created);
        return createdDate >= cover.StartDate && createdDate <= cover.EndDate;
    }
}
