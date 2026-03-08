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
    public Task<decimal> ComputeAsync(DateOnly startDate, DateOnly endDate, CoverType coverType,
        IPremiumPolicy policy, CancellationToken cancellation = default)
    {
        return Task.FromResult(Compute(startDate, endDate, coverType, policy, cancellation));
    }

    private decimal Compute(DateOnly startDate, DateOnly endDate, CoverType coverType,
        IPremiumPolicy policy, CancellationToken cancellation)
    {
        var multiplier = policy.GetTypeMultiplier(coverType);
        var premiumPerDay = policy.BaseDayRate * multiplier;
        var totalDays = endDate.DayNumber - startDate.DayNumber;
        if (totalDays <= 0)
        {
            return 0;
        }

        decimal total = 0;
        for (var dayIndex = 0; dayIndex < totalDays; dayIndex++)
        {
            var dailyRate = premiumPerDay * policy.GetDayDiscount(dayIndex, coverType);
            total += dailyRate;
        }

        return total;
    }
}
