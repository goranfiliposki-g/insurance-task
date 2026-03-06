using Claims.Domain.Entities;

namespace Claims.Application.DTOs;

public record CreateCoverDto(DateTime StartDate, DateTime EndDate, CoverType Type);
