using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class CommonDbContext : DbContext
{
    public CommonDbContext(DbContextOptions<CommonDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommonDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
