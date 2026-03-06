using Claims.Application.Common;
using Claims.Application.DTOs;
using Claims.Application.Interfaces;
using Claims.Application.Services;
using Claims.Domain.Entities;
using Moq;
using Xunit;

namespace Claims.Tests;

public class CoverServiceValidationTests
{
    [Fact]
    public async Task CreateAsync_Throws_When_StartDate_In_Past()
    {
        var repo = new Mock<ICoverRepository>();
        var audit = new Mock<IAuditService>();
        var calculator = new PremiumCalculator();
        var service = new CoverService(repo.Object, audit.Object, calculator);
        var dto = new CreateCoverDto(
            StartDate: DateTime.UtcNow.AddDays(-1),
            EndDate: DateTime.UtcNow.AddDays(30),
            Type: CoverType.Yacht
        );

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_Throws_When_Period_Exceeds_One_Year()
    {
        var repo = new Mock<ICoverRepository>();
        var audit = new Mock<IAuditService>();
        var calculator = new PremiumCalculator();
        var service = new CoverService(repo.Object, audit.Object, calculator);
        var dto = new CreateCoverDto(
            StartDate: DateTime.UtcNow.Date,
            EndDate: DateTime.UtcNow.Date.AddDays(366),
            Type: CoverType.Yacht
        );

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_Succeeds_When_Valid()
    {
        var repo = new Mock<ICoverRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<Cover>())).Returns(Task.CompletedTask);
        var audit = new Mock<IAuditService>();
        var calculator = new PremiumCalculator();
        var service = new CoverService(repo.Object, audit.Object, calculator);
        var dto = new CreateCoverDto(
            StartDate: DateTime.UtcNow.Date,
            EndDate: DateTime.UtcNow.Date.AddDays(30),
            Type: CoverType.Yacht
        );

        var id = await service.CreateAsync(dto);

        Assert.NotNull(id);
        Assert.NotEmpty(id);
    }
}
