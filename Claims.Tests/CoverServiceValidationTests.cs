using Claims.Application.Common;
using Claims.Application.DTOs;
using Claims.Application.Interfaces;
using Claims.Application.Services;
using Claims.Application.Validators;
using Claims.Domain.Entities;
using Claims.Domain.Enums;
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
        var calculator = new PremiumCalculator(new DefaultPremiumPolicy());
        var validator = new CreateCoverDtoValidator();
        var service = new CoverService(repo.Object, audit.Object, calculator, validator);
        var dto = new CreateCoverDto(
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            EndDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            Type: CoverType.Yacht
        );

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_Throws_When_Period_Exceeds_One_Year()
    {
        var repo = new Mock<ICoverRepository>();
        var audit = new Mock<IAuditService>();
        var calculator = new PremiumCalculator(new DefaultPremiumPolicy());
        var validator = new CreateCoverDtoValidator();
        var service = new CoverService(repo.Object, audit.Object, calculator, validator);
        var dto = new CreateCoverDto(
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate: DateOnly.FromDateTime(DateTime.UtcNow).AddDays(366),
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
        var calculator = new PremiumCalculator(new DefaultPremiumPolicy());
        var validator = new CreateCoverDtoValidator();
        var service = new CoverService(repo.Object, audit.Object, calculator, validator);
        var dto = new CreateCoverDto(
            StartDate: DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate: DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30),
            Type: CoverType.Yacht
        );

        var id = await service.CreateAsync(dto);

        Assert.NotNull(id);
        Assert.NotEmpty(id);
    }

    [Fact]
    public async Task CreateAsync_Throws_When_EndDate_Before_StartDate()
    {
        var repo = new Mock<ICoverRepository>();
        var audit = new Mock<IAuditService>();
        var calculator = new PremiumCalculator(new DefaultPremiumPolicy());
        var validator = new CreateCoverDtoValidator();
        var service = new CoverService(repo.Object, audit.Object, calculator, validator);
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var dto = new CreateCoverDto(
            StartDate: startDate,
            EndDate: startDate.AddDays(-1),
            Type: CoverType.Yacht
        );

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(dto));
        Assert.Contains("EndDate must be after StartDate.", ex.Errors);
    }
}
