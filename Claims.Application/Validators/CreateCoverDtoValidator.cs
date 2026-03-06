using Claims.Application.Common;
using Claims.Application.DTOs;
using FluentValidation;

namespace Claims.Application.Validators;

/// <summary>Validates cover period in calendar days (DateOnly; no time-of-day).</summary>
public class CreateCoverDtoValidator : AbstractValidator<CreateCoverDto>
{
    public CreateCoverDtoValidator()
    {
        RuleFor(x => x.StartDate)
            .Must(BeNotInPast)
            .WithMessage("StartDate cannot be in the past.");

        RuleFor(x => x)
            .Must(HaveEndAfterStart)
            .WithMessage("EndDate must be after StartDate.");

        RuleFor(x => x)
            .Must(HavePeriodWithinOneYear)
            .WithMessage("Total insurance period cannot exceed 1 year.");
    }

    private static bool BeNotInPast(DateOnly date) => date >= DateOnly.FromDateTime(DateTime.UtcNow);

    private static bool HaveEndAfterStart(CreateCoverDto dto) => dto.EndDate >= dto.StartDate;

    private static bool HavePeriodWithinOneYear(CreateCoverDto dto) =>
        (dto.EndDate.DayNumber - dto.StartDate.DayNumber) <= CoverValidationConstants.MaxInsuranceDays;
}
