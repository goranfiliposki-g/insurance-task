using System.Net.Http.Json;
using Claims.API;
using Claims.Application.DTOs;
using Claims.Domain.Enums;
using Claims.Infrastructure.Auditing;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Claims.Tests;

public class AuditPipelineTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuditPipelineTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private static async Task WaitForAuditAsync(int milliseconds = 200)
    {
        await Task.Delay(milliseconds);
    }

    [Fact]
    public async Task Creating_Claim_Writes_ClaimAudit_With_POST()
    {
        var client = _factory.CreateClient();
        var coverId = await CreateCoverAndGetId(client);
        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        var claimDto = new CreateClaimDto(coverId, start.AddDays(5).ToDateTime(TimeOnly.MinValue), "Audit Test", ClaimType.Fire, 1000);
        var createResponse = await client.PostAsJsonAsync("/api/Claims", claimDto);
        createResponse.EnsureSuccessStatusCode();
        var claimId = (await createResponse.Content.ReadFromJsonAsync<CreateResponse>())!.Id;

        await WaitForAuditAsync();

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var audits = await db.ClaimAudits.Where(a => a.ClaimId == claimId).ToListAsync();
        var createAudit = Assert.Single(audits);
        Assert.Equal("POST", createAudit.HttpRequestType);
    }

    [Fact]
    public async Task Deleting_Claim_Writes_ClaimAudit_With_DELETE()
    {
        var client = _factory.CreateClient();
        var coverId = await CreateCoverAndGetId(client);
        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        var claimDto = new CreateClaimDto(coverId, start.AddDays(5).ToDateTime(TimeOnly.MinValue), "Audit Delete Test", ClaimType.Fire, 1000);
        var createResponse = await client.PostAsJsonAsync("/api/Claims", claimDto);
        createResponse.EnsureSuccessStatusCode();
        var claimId = (await createResponse.Content.ReadFromJsonAsync<CreateResponse>())!.Id;

        await WaitForAuditAsync();

        var deleteResponse = await client.DeleteAsync($"/api/Claims/{claimId}");
        deleteResponse.EnsureSuccessStatusCode();

        await WaitForAuditAsync();

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var audits = await db.ClaimAudits.Where(a => a.ClaimId == claimId).OrderBy(a => a.Created).ToListAsync();
        Assert.Equal(2, audits.Count);
        Assert.Equal("POST", audits[0].HttpRequestType);
        Assert.Equal("DELETE", audits[1].HttpRequestType);
    }

    [Fact]
    public async Task Creating_Cover_Writes_CoverAudit_With_POST()
    {
        var client = _factory.CreateClient();
        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        var end = start.AddDays(30);
        var dto = new CreateCoverDto(start, end, CoverType.Yacht);
        var createResponse = await client.PostAsJsonAsync("/api/Covers", dto);
        createResponse.EnsureSuccessStatusCode();
        var coverId = (await createResponse.Content.ReadFromJsonAsync<CreateResponse>())!.Id;

        await WaitForAuditAsync();

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var audits = await db.CoverAudits.Where(a => a.CoverId == coverId).ToListAsync();
        var createAudit = Assert.Single(audits);
        Assert.Equal("POST", createAudit.HttpRequestType);
    }

    [Fact]
    public async Task Deleting_Cover_Writes_CoverAudit_With_DELETE()
    {
        var client = _factory.CreateClient();
        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        var end = start.AddDays(30);
        var dto = new CreateCoverDto(start, end, CoverType.Tanker);
        var createResponse = await client.PostAsJsonAsync("/api/Covers", dto);
        createResponse.EnsureSuccessStatusCode();
        var coverId = (await createResponse.Content.ReadFromJsonAsync<CreateResponse>())!.Id;

        await WaitForAuditAsync();

        var deleteResponse = await client.DeleteAsync($"/api/Covers/{coverId}");
        deleteResponse.EnsureSuccessStatusCode();

        await WaitForAuditAsync();

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var audits = await db.CoverAudits.Where(a => a.CoverId == coverId).OrderBy(a => a.Created).ToListAsync();
        Assert.Equal(2, audits.Count);
        Assert.Equal("POST", audits[0].HttpRequestType);
        Assert.Equal("DELETE", audits[1].HttpRequestType);
    }

    private static async Task<string> CreateCoverAndGetId(HttpClient client)
    {
        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        var end = start.AddDays(30);
        var coverDto = new CreateCoverDto(start, end, CoverType.Yacht);
        var createCoverResponse = await client.PostAsJsonAsync("/api/Covers", coverDto);
        createCoverResponse.EnsureSuccessStatusCode();
        var body = await createCoverResponse.Content.ReadFromJsonAsync<CreateResponse>();
        return body!.Id;
    }
}
