using Claims.API.Models;
using Claims.Application.DTOs;
using Claims.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Claims.API.Controllers;

/// <summary>CRUD and premium computation for insurance covers.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CoversController : ControllerBase
{
    private readonly ICoverService _service;

    public CoversController(ICoverService service)
    {
        _service = service;
    }

    /// <summary>Create a new cover. Validates StartDate not in past and period not exceeding 1 year. Premium is computed automatically.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCoverDto dto)
    {
        var id = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>Get a cover by id.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CoverDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var cover = await _service.GetByIdAsync(id);
        return cover == null ? NotFound() : Ok(cover.ToDto());
    }

    /// <summary>List all covers.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CoverDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
        => Ok((await _service.GetAllAsync()).Select(c => c.ToDto()).ToList());

    /// <summary>Delete a cover by id. Returns 404 if not found.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Compute premium for given period and cover type (no persistence). Period is in calendar days (date-only). Cover type: Yacht, PassengerShip, ContainerShip, BulkCarrier, Tanker.</summary>
    [HttpPost("compute")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ComputePremium([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] string coverType, CancellationToken cancellationToken = default)
        => Ok(await _service.ComputePremiumAsync(startDate, endDate, coverType, cancellationToken));
}
