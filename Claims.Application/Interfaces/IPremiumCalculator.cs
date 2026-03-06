using Claims.Domain.Entities;

namespace Claims.Application.Interfaces;

public interface IPremiumCalculator
{
    decimal Compute(DateTime startDate, DateTime endDate, CoverType coverType);
}
