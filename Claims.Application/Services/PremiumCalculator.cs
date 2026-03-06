using Claims.Application.Interfaces;
using Claims.Domain.Enums;

namespace Claims.Application.Services;

/// <summary>
/// Computes premium from an injected policy.
/// Uses calendar-day semantics: start and end are dates only (no time-of-day).
/// The period length is the number of calendar days between start and end (exclusive of end date).
/// Day index 0 is the first day of the period, 1 the second, etc.
/// </summary>
public class PremiumCalculator : IPremiumCalculator
{
    private readonly IPremiumPolicy _policy;

    public PremiumCalculator(IPremiumPolicy policy)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
    }

    public decimal Compute(DateOnly startDate, DateOnly endDate, CoverType coverType)
    {
        var multiplier = _policy.GetTypeMultiplier(coverType);
        var premiumPerDay = _policy.BaseDayRate * multiplier;
        var totalDays = endDate.DayNumber - startDate.DayNumber;
        if (totalDays <= 0)
        {
            return 0;
        }

        decimal total = 0;
        for (var dayIndex = 0; dayIndex < totalDays; dayIndex++)
        {
            var dailyRate = premiumPerDay * _policy.GetDayDiscount(dayIndex, coverType);
            total += dailyRate;
        }

        return total;
    }
}
