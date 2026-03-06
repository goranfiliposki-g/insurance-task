using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Auditing;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    public DbSet<ClaimAudit> ClaimAudits => Set<ClaimAudit>();
    public DbSet<CoverAudit> CoverAudits => Set<CoverAudit>();
}
