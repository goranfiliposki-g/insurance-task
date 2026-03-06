using Claims.API.Models;
using Claims.Application.DTOs;
using Claims.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Claims.API.Controllers;

/// <summary>CRUD and query for insurance claims.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ClaimsController : ControllerBase
{
    private readonly IClaimService _service;

    public ClaimsController(IClaimService service)
    {
        _service = service;
    }

    /// <summary>Create a new claim. Validates DamageCost (max 100,000) and that Created is within the related Cover period.</summary>
    /// <returns>201 with location header and new claim id.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateClaimDto dto)
    {
        var id = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>Get a claim by id.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClaimDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var claim = await _service.GetByIdAsync(id);
        return claim == null ? NotFound() : Ok(claim.ToDto());
    }

    /// <summary>List all claims.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClaimDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
        => Ok((await _service.GetAllAsync()).Select(c => c.ToDto()).ToList());

    /// <summary>Delete a claim by id. Returns 404 if not found.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
