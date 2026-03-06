using Claims.Domain.Enums;

namespace Claims.Application.DTOs;

/// <summary>Cover period uses calendar days only (no time-of-day).</summary>
public record CreateCoverDto(DateOnly StartDate, DateOnly EndDate, CoverType Type);
