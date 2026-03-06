using Claims.Domain.Enums;

namespace Claims.Domain.Entities;

/// <summary>Insurance cover entity.</summary>
public class Cover
{
    public string Id { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public CoverType Type { get; set; }
    public decimal Premium { get; set; }
}


