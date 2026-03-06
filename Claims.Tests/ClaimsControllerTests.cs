using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Claims.API;
using Claims.Application.DTOs;
using Claims.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Claims.Tests;

internal sealed record CreateResponse([property: JsonPropertyName("id")] string Id);

public class ClaimsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ClaimsControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
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

    [Fact]
    public async Task Get_Claims_Returns_200_And_Array()
    {
        var response = await _client.GetAsync("/api/Claims");
        response.EnsureSuccessStatusCode();
        var claims = await response.Content.ReadFromJsonAsync<ClaimDto[]>();
        Assert.NotNull(claims);
        Assert.IsAssignableFrom<IReadOnlyList<ClaimDto>>(claims);
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

    [Fact]
    public async Task Get_Claim_By_Id_Returns_200_And_Claim_When_Found()
    {
        var coverId = await CreateCoverAndGetId(_client);

        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        var claimDto = new CreateClaimDto(coverId, start.AddDays(5).ToDateTime(TimeOnly.MinValue), "GetById Test", ClaimType.Fire, 5000);
        var createClaimResponse = await _client.PostAsJsonAsync("/api/Claims", claimDto);
        createClaimResponse.EnsureSuccessStatusCode();
        var claimId = (await createClaimResponse.Content.ReadFromJsonAsync<CreateResponse>())!.Id;

        var getResponse = await _client.GetAsync($"/api/Claims/{claimId}");
        getResponse.EnsureSuccessStatusCode();
        var claim = await getResponse.Content.ReadFromJsonAsync<ClaimDto>();
        Assert.NotNull(claim);
        Assert.Equal(claimId, claim.Id);
        Assert.Equal(coverId, claim.CoverId);
        Assert.Equal("GetById Test", claim.Name);
        Assert.Equal(ClaimType.Fire, claim.Type);
        Assert.Equal(5000, claim.DamageCost);
    }

    [Fact]
    public async Task Delete_Claim_Returns_204_When_Found()
    {
        var coverId = await CreateCoverAndGetId(_client);

        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        var claimDto = new CreateClaimDto(coverId, start.AddDays(5).ToDateTime(TimeOnly.MinValue), "Delete Test", ClaimType.Fire, 1000);
        var createClaimResponse = await _client.PostAsJsonAsync("/api/Claims", claimDto);
        createClaimResponse.EnsureSuccessStatusCode();
        var claimId = (await createClaimResponse.Content.ReadFromJsonAsync<CreateResponse>())!.Id;

        var deleteResponse = await _client.DeleteAsync($"/api/Claims/{claimId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDelete = await _client.GetAsync($"/api/Claims/{claimId}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    [Fact]
    public async Task Delete_Claim_Returns_404_When_Not_Found()
    {
        var response = await _client.DeleteAsync("/api/Claims/nonexistent-claim-id");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        var end = start.AddDays(30);
        var response = await _client.PostAsync($"/api/Covers/compute?startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}&coverType=Yacht", null);
        response.EnsureSuccessStatusCode();
        var premium = await response.Content.ReadFromJsonAsync<decimal>();
        Assert.True(premium > 0);
    }

    [Fact]
    public async Task Get_Cover_By_Id_Returns_200_And_Cover_When_Found()
    {
        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        var end = start.AddDays(30);
        var dto = new CreateCoverDto(start, end, CoverType.Yacht);
        var createResponse = await _client.PostAsJsonAsync("/api/Covers", dto);
        createResponse.EnsureSuccessStatusCode();
        var id = (await createResponse.Content.ReadFromJsonAsync<CreateResponse>())!.Id;

        var getResponse = await _client.GetAsync($"/api/Covers/{id}");
        getResponse.EnsureSuccessStatusCode();
        var cover = await getResponse.Content.ReadFromJsonAsync<CoverDto>();
        Assert.NotNull(cover);
        Assert.Equal(id, cover.Id);
        Assert.Equal(start, cover.StartDate);
        Assert.Equal(end, cover.EndDate);
        Assert.Equal(CoverType.Yacht, cover.Type);
        Assert.True(cover.Premium > 0);
    }

    [Fact]
    public async Task Delete_Cover_Returns_204_When_Found()
    {
        var start = DateOnly.FromDateTime(DateTime.UtcNow);
        var end = start.AddDays(30);
        var dto = new CreateCoverDto(start, end, CoverType.Tanker);
        var createResponse = await _client.PostAsJsonAsync("/api/Covers", dto);
        createResponse.EnsureSuccessStatusCode();
        var id = (await createResponse.Content.ReadFromJsonAsync<CreateResponse>())!.Id;

        var deleteResponse = await _client.DeleteAsync($"/api/Covers/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDelete = await _client.GetAsync($"/api/Covers/{id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    [Fact]
    public async Task Delete_Cover_Returns_404_When_Not_Found()
    {
        var response = await _client.DeleteAsync("/api/Covers/nonexistent-cover-id");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
