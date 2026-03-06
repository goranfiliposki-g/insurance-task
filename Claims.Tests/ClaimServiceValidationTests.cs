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

public class ClaimServiceValidationTests
{
    [Fact]
    public async Task CreateAsync_Throws_When_DamageCost_Exceeds_100000()
    {
        var repo = new Mock<IClaimRepository>();
        var coverRepo = new Mock<ICoverRepository>();
        var cover = new Cover
        {
            Id = "c1",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)),
            Type = CoverType.Yacht,
            Premium = 1000
        };
        coverRepo.Setup(r => r.GetByIdAsync("c1")).ReturnsAsync(cover);
        var audit = new Mock<IAuditService>();
        var validator = new CreateClaimDtoValidator(coverRepo.Object);
        var service = new ClaimService(repo.Object, coverRepo.Object, audit.Object, validator);
        var dto = new CreateClaimDto(
            CoverId: "c1",
            Created: DateTime.UtcNow,
            Name: "Test",
            Type: ClaimType.Fire,
            DamageCost: 100_001
        );

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_Throws_When_Created_Outside_Cover_Period()
    {
        var repo = new Mock<IClaimRepository>();
        var coverRepo = new Mock<ICoverRepository>();
        var cover = new Cover
        {
            Id = "c1",
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 1, 31),
            Type = CoverType.Yacht,
            Premium = 1000
        };
        coverRepo.Setup(r => r.GetByIdAsync("c1")).ReturnsAsync(cover);
        var audit = new Mock<IAuditService>();
        var validator = new CreateClaimDtoValidator(coverRepo.Object);
        var service = new ClaimService(repo.Object, coverRepo.Object, audit.Object, validator);
        var dto = new CreateClaimDto(
            CoverId: "c1",
            Created: new DateTime(2025, 2, 15), // outside cover
            Name: "Test",
            Type: ClaimType.Fire,
            DamageCost: 1000
        );

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_Succeeds_When_Valid()
    {
        var repo = new Mock<IClaimRepository>();
        repo.Setup(r => r.AddAsync(It.IsAny<Claim>())).Returns(Task.CompletedTask);
        var coverRepo = new Mock<ICoverRepository>();
        var cover = new Cover
        {
            Id = "c1",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)),
            Type = CoverType.Yacht,
            Premium = 1000
        };
        coverRepo.Setup(r => r.GetByIdAsync("c1")).ReturnsAsync(cover);
        var audit = new Mock<IAuditService>();
        var validator = new CreateClaimDtoValidator(coverRepo.Object);
        var service = new ClaimService(repo.Object, coverRepo.Object, audit.Object, validator);
        var dto = new CreateClaimDto(
            CoverId: "c1",
            Created: DateTime.UtcNow,
            Name: "Test",
            Type: ClaimType.Fire,
            DamageCost: 50_000
        );

        var id = await service.CreateAsync(dto);

        Assert.NotNull(id);
        Assert.NotEmpty(id);
    }
}
