using Claims.Application.Interfaces;
using Claims.Infrastructure.Auditing;
using Claims.Infrastructure.Persistence;
using Claims.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Claims.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registers claims and audit persistence. Uses InMemory by default.</summary>
    public static IServiceCollection AddClaimsInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<ClaimsDbContext>(opt => opt.UseInMemoryDatabase("ClaimsDb"));
        services.AddDbContext<AuditDbContext>(opt => opt.UseInMemoryDatabase("AuditDb"));

        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<ICoverRepository, CoverRepository>();
        services.AddSingleton<IAuditService, AuditService>();
        services.AddHostedService(sp => (AuditService)sp.GetRequiredService<IAuditService>());

        return services;
    }
}
