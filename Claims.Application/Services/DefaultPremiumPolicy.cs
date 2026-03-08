using Claims.Application.Interfaces;
using Claims.Domain.Enums;

namespace Claims.Application.Services
{
    public sealed class DefaultPremiumPolicy : IPremiumPolicy
    {
        public decimal BaseDayRate => 1250m;
        public int FirstTierDayCount => 30;
        public int SecondTierEndDayExclusive => 180;

        public decimal GetTypeMultiplier(CoverType coverType) => coverType switch
        {
            CoverType.Yacht => 1.10m,
            CoverType.PassengerShip => 1.20m,
            CoverType.Tanker => 1.50m,
            CoverType.ContainerShip or CoverType.BulkCarrier => 1.30m,
            _ => 1.30m
        };

        public decimal GetDayDiscount(int dayIndex, CoverType coverType)
        {
            if (dayIndex < FirstTierDayCount)
            {
                return 1.00m;
            }

            if (dayIndex < SecondTierEndDayExclusive)
            {
                return coverType == CoverType.Yacht ? 0.95m : 0.98m;
            }

            return coverType == CoverType.Yacht ? 0.97m : 0.99m;
        }
    }
}
