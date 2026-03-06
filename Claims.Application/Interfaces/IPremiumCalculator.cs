using Claims.Domain.Enums;

namespace Claims.Application.Interfaces;

/// <summary>Premium is computed over a calendar-day period (start and end are dates only; time-of-day is ignored).</summary>
public interface IPremiumCalculator
{
    decimal Compute(DateOnly startDate, DateOnly endDate, CoverType coverType);
}
