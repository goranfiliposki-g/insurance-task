using Claims.Domain.Enums;

namespace Claims.Application.DTOs;

public record CoverDto(string Id, DateOnly StartDate, DateOnly EndDate, CoverType Type, decimal Premium);
