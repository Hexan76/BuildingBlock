using Microsoft.EntityFrameworkCore;

using Volo.Abp.EntityFrameworkCore;

namespace Framework.BuildingBlock.EntityFrameworkCore;

public abstract class GenericDbContextWrapper<TDbContext>
    : AbpDbContext<TDbContext>, IEFCoreDbContextFramework
    where TDbContext : DbContext
{
    protected GenericDbContextWrapper(DbContextOptions<TDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        ConfigureFramework(optionsBuilder);
    }

    protected virtual void ConfigureFramework(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureFrameworkModels(builder);
    }

    protected virtual void ConfigureFrameworkModels(ModelBuilder builder)
    {

    }
}
