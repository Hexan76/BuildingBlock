using Framework.BuildingBlock.Domain;
using Microsoft.EntityFrameworkCore;

using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Framework.BuildingBlock.EntityFrameworkCore;

[ConnectionStringName(BuildingBlockDbProperties.ConnectionStringName)]
public class BuildingBlockDbContext : AbpDbContext<BuildingBlockDbContext>, IBuildingBlockDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */

    public BuildingBlockDbContext(DbContextOptions<BuildingBlockDbContext> options)
        : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies()
        ;
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureBuildingBlock();
    }
}
