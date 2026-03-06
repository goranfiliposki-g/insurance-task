using System.Net;
using System.Net.Http.Json;
using Claims.API;
using Claims.Application.DTOs;
using Claims.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Claims.Tests;

public class ClaimsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ClaimsControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Claims_Returns_200_And_Array()
    {
        var response = await _client.GetAsync("/api/Claims");
        response.EnsureSuccessStatusCode();
        var claims = await response.Content.ReadFromJsonAsync<ClaimDto[]>();
        Assert.NotNull(claims);
        Assert.IsAssignableFrom<IEnumerable<ClaimDto>>(claims);
    }

    [Fact]
    public async Task Get_Claim_By_Id_Returns_404_When_Not_Found()
    {
        var response = await _client.GetAsync("/api/Claims/nonexistent-id");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Claim_Without_Valid_Cover_Returns_400()
    {
        var dto = new CreateClaimDto(
            CoverId: "non-existent-cover",
            Created: DateTime.UtcNow,
            Name: "Test",
            Type: ClaimType.Fire,
            DamageCost: 1000
        );
        var response = await _client.PostAsJsonAsync("/api/Claims", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public class CoversControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CoversControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Covers_Returns_200_And_Array()
    {
        var response = await _client.GetAsync("/api/Covers");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        Assert.NotNull(json);
    }

    [Fact]
    public async Task ComputePremium_Returns_200_And_Number()
    {
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(30);
        var response = await _client.PostAsync($"/api/Covers/compute?startDate={start:O}&endDate={end:O}&coverType=Yacht", null);
        response.EnsureSuccessStatusCode();
        var premium = await response.Content.ReadFromJsonAsync<decimal>();
        Assert.True(premium > 0);
    }
}
