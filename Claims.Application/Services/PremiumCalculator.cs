using Claims.Application.Interfaces;
using Claims.Domain.Entities;

namespace Claims.Application.Services;

/// <summary>
/// Computes insurance premium - base 1250; type multipliers (Yacht +10%, Passenger +20%, Tanker +50%, other +30%);
/// first 30 days full rate; next 150 days Yacht -5%, others -2%; remaining days Yacht -3%, others -1%.
/// </summary>
public class PremiumCalculator : IPremiumCalculator
{
    private const decimal BaseDayRate = 1250m;

    public decimal Compute(DateTime startDate, DateTime endDate, CoverType coverType)
    {
        var multiplier = GetTypeMultiplier(coverType);
        var premiumPerDay = BaseDayRate * multiplier;
        var totalDays = (int)(endDate - startDate).TotalDays;
        if (totalDays <= 0)
        {
            return 0;
        }
        
        decimal total = 0;
        for (var dayIndex = 0; dayIndex < totalDays; dayIndex++)
        {
            var dailyRate = premiumPerDay * GetDayDiscount(dayIndex, coverType);
            total += dailyRate;
        }

        return total;
    }

    private decimal GetTypeMultiplier(CoverType coverType) => coverType switch
    {
        CoverType.Yacht => 1.10m,
        CoverType.PassengerShip => 1.20m,
        CoverType.Tanker => 1.50m,
        CoverType.ContainerShip or CoverType.BulkCarrier => 1.30m,
        _ => 1.30m
    };

    /// <summary>First 30 days full; next 150 days (31-180) Yacht 0.95, others 0.98; remaining 0.97 / 0.99.</summary>
    private decimal GetDayDiscount(int dayIndex, CoverType coverType)
    {
        if (dayIndex < 30)
        {
            return 1.00m;
        }

        if (dayIndex < 180)
        { 
            return coverType == CoverType.Yacht ? 0.95m : 0.98m; 
        }

        return coverType == CoverType.Yacht ? 0.97m : 0.99m;
    }
}
