using Claims.Domain.Entities;

namespace Claims.Application.DTOs;

public record CoverDto(string Id, DateTime StartDate, DateTime EndDate, CoverType Type, decimal Premium);
