using Claims.Application.DTOs;
using Claims.Domain.Entities;

namespace Claims.API.Models;

internal static class ClaimMapping
{
    public static ClaimDto ToDto(this Claim c) =>
        new(c.Id, c.CoverId, c.Created, c.Name, c.Type, c.DamageCost);
}
