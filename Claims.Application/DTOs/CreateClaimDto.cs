using Claims.Domain.Enums;

namespace Claims.Application.DTOs;

public record CreateClaimDto(
    string CoverId,
    DateTime Created,
    string Name,
    ClaimType Type,
    decimal DamageCost
);
