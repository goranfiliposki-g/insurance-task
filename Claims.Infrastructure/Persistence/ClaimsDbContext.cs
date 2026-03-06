using Claims.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Persistence;

public class ClaimsDbContext : DbContext
{
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<Cover> Covers => Set<Cover>();

    public ClaimsDbContext(DbContextOptions<ClaimsDbContext> options)
        : base(options) { }
}
