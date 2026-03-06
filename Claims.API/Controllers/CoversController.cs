using Claims.Application.DTOs;
using Claims.Application.Interfaces;
using Claims.Domain.Entities;
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
        return cover == null ? NotFound() : Ok(cover);
    }

    /// <summary>List all covers.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CoverDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    /// <summary>Delete a cover by id.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(string id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Compute premium for given period and cover type (no persistence).</summary>
    [HttpPost("compute")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    public IActionResult ComputePremium([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] CoverType coverType)
        => Ok(_service.ComputePremium(startDate, endDate, coverType));
}
