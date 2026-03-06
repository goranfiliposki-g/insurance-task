using Claims.Application.DTOs;
using Claims.Domain.Entities;

namespace Claims.API.Models;

internal static class CoverMapping
{
    public static CoverDto ToDto(this Cover c) =>
        new(c.Id, c.StartDate, c.EndDate, c.Type, c.Premium);
}
