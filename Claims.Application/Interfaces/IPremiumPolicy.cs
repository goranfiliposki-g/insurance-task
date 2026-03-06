using Claims.Domain.Enums;

/// <summary>Premium calculation rules (base rate, tiers, type multipliers); inject to support multiple policies. Day index is 0-based calendar day (0 = first day of period).</summary>
namespace Claims.Application.Interfaces
{
    public interface IPremiumPolicy
    {
        decimal BaseDayRate { get; }
        int FirstTierDayCount { get; }
        int SecondTierEndDayExclusive { get; }
        decimal GetTypeMultiplier(CoverType coverType);
        decimal GetDayDiscount(int dayIndex, CoverType coverType);
    }
}
