using Claims.Application.Services;
using Claims.Domain.Enums;
using Xunit;

namespace Claims.Tests;

public class PremiumCalculatorTests
{
    private readonly PremiumCalculator _calculator = new();
    private readonly DefaultPremiumPolicy _defaultPolicy = new DefaultPremiumPolicy();

    [Fact]
    public async Task Compute_Zero_Days_Returns_Zero()
    {
        var start = new DateOnly(2025, 1, 1);
        var result = await _calculator.ComputeAsync(start, start, CoverType.Yacht, _defaultPolicy, TestContext.Current.CancellationToken);
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task Compute_30_Days_Yacht_Base_Rate_Times_1_1()
    {
        var start = new DateOnly(2025, 1, 1);
        var end = start.AddDays(30);
        var result = await _calculator.ComputeAsync(start, end, CoverType.Yacht, _defaultPolicy, TestContext.Current.CancellationToken);
        var expected = 30 * 1250m * 1.10m;
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task Compute_30_Days_Tanker_Base_Rate_Times_1_5()
    {
        var start = new DateOnly(2025, 1, 1);
        var end = start.AddDays(30);
        var result = await _calculator.ComputeAsync(start, end, CoverType.Tanker, _defaultPolicy, TestContext.Current.CancellationToken);
        var expected = 30 * 1250m * 1.50m;
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task Compute_180_Days_Includes_First_30_Full_Then_150_SecondTier()
    {
        var start = new DateOnly(2025, 1, 1);
        var end = start.AddDays(180);
        var result = await _calculator.ComputeAsync(start, end, CoverType.Yacht, _defaultPolicy, TestContext.Current.CancellationToken);
        var premiumPerDay = 1250m * 1.10m;
        var first30 = 30 * premiumPerDay;
        var days31to180 = 150 * premiumPerDay * 0.95m;
        
        var expected = first30 + days31to180;
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task Compute_365_Days_Includes_All_Three_Tiers()
    {
        var start = new DateOnly(2025, 1, 1);
        var end = start.AddDays(365);
        var result = await _calculator.ComputeAsync(start, end, CoverType.PassengerShip, _defaultPolicy, TestContext.Current.CancellationToken);
        var premiumPerDay = 1250m * 1.20m;
        var first30 = 30 * premiumPerDay;
        var days31to180 = 150 * premiumPerDay * 0.98m;
        var days181to364 = 185 * premiumPerDay * 0.99m;
        var expected = first30 + days31to180 + days181to364;
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task Compute_Single_Day_Returns_One_Day_Full_Rate()
    {
        var start = new DateOnly(2025, 1, 1);
        var end = start.AddDays(1);
        var result = await _calculator.ComputeAsync(start, end, CoverType.Yacht, _defaultPolicy, TestContext.Current.CancellationToken);
        var expected = 1250m * 1.10m;
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task Compute_Day_30_Boundary_Last_Day_Full_Rate()
    {
        var start = new DateOnly(2025, 1, 1);
        var result30 = await _calculator.ComputeAsync(start, start.AddDays(30), CoverType.Yacht, _defaultPolicy, TestContext.Current.CancellationToken);
        var result31 = await _calculator.ComputeAsync(start, start.AddDays(31), CoverType.Yacht, _defaultPolicy, TestContext.Current.CancellationToken);
        var premiumPerDay = 1250m * 1.10m;
        var expected30 = 30 * premiumPerDay;
        var expected31 = 30 * premiumPerDay + 1 * premiumPerDay * 0.95m;
        Assert.Equal(expected30, result30);
        Assert.Equal(expected31, result31);
    }
    
    [Fact]
    public async Task Compute_Day_180_Boundary_Third_Tier_Starts_At_Day_181()
    {
        var start = new DateOnly(2025, 1, 1);
        var premiumPerDay = 1250m * 1.10m;
        var result180 = await _calculator.ComputeAsync(start, start.AddDays(180), CoverType.Yacht, _defaultPolicy, TestContext.Current.CancellationToken);
        var result181 = await _calculator.ComputeAsync(start, start.AddDays(181), CoverType.Yacht, _defaultPolicy, TestContext.Current.CancellationToken);
        var expected180 = 30 * premiumPerDay + 150 * premiumPerDay * 0.95m;
        var expected181 = expected180 + 1 * premiumPerDay * 0.97m;
        Assert.Equal(expected180, result180);
        Assert.Equal(expected181, result181);
    }
}
