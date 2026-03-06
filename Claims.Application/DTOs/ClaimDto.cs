using Claims.Domain.Entities;
using Claims.Domain.Enums;

namespace Claims.Application.DTOs;

public record ClaimDto(
    string Id,
    string CoverId,
    DateTime Created,
    string Name,
    ClaimType Type,
    decimal DamageCost
);
