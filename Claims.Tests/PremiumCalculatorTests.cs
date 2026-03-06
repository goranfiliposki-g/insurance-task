using Claims.Application.Services;
using Claims.Domain.Entities;
using Xunit;

namespace Claims.Tests;

public class PremiumCalculatorTests
{
    private readonly PremiumCalculator _calculator = new();

    [Fact]
    public void Compute_Zero_Days_Returns_Zero()
    {
        var start = new DateTime(2025, 1, 1);
        var result = _calculator.Compute(start, start, CoverType.Yacht);
        Assert.Equal(0, result);
    }

    [Fact]
    public void Compute_30_Days_Yacht_Base_Rate_Times_1_1()
    {
        var start = new DateTime(2025, 1, 1);
        var end = start.AddDays(30);
        var result = _calculator.Compute(start, end, CoverType.Yacht);
        // 30 days at 1250 * 1.10 = 1375 per day = 41250
        var expected = 30 * 1250m * 1.10m;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Compute_30_Days_Tanker_Base_Rate_Times_1_5()
    {
        var start = new DateTime(2025, 1, 1);
        var end = start.AddDays(30);
        var result = _calculator.Compute(start, end, CoverType.Tanker);
        var expected = 30 * 1250m * 1.50m;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Compute_180_Days_Includes_First_30_Full_Then_150_Discounted()
    {
        var start = new DateTime(2025, 1, 1);
        var end = start.AddDays(180);
        var result = _calculator.Compute(start, end, CoverType.Yacht);
        var premiumPerDay = 1250m * 1.10m;
        var first30 = 30 * premiumPerDay;
        var next150 = 150 * premiumPerDay * 0.95m;
        var expected = first30 + next150;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Compute_365_Days_Includes_All_Three_Tiers()
    {
        var start = new DateTime(2025, 1, 1);
        var end = start.AddDays(365);
        var result = _calculator.Compute(start, end, CoverType.PassengerShip);
        var premiumPerDay = 1250m * 1.20m;
        var first30 = 30 * premiumPerDay;
        var days31to180 = 150 * premiumPerDay * 0.98m;
        var days181to364 = 185 * premiumPerDay * 0.99m;
        var expected = first30 + days31to180 + days181to364;
        Assert.Equal(expected, result);
    }
}
