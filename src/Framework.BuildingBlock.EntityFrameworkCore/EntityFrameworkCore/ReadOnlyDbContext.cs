using Microsoft.EntityFrameworkCore;

using Volo.Abp.EntityFrameworkCore;

namespace Framework.BuildingBlock.EntityFrameworkCore;

public abstract class ReadOnlyDbContext<TDbContext>
    : AbpDbContext<TDbContext>
    where TDbContext : DbContext
{
    protected ReadOnlyDbContext(DbContextOptions<TDbContext> options)
        : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureModels(builder);
    }

    protected virtual void ConfigureModels(ModelBuilder builder)
    {
    }

    public override int SaveChanges()
        => ThrowReadOnly();

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
        => ThrowReadOnlyAsync();

    private static int ThrowReadOnly()
        => throw new InvalidOperationException("This DbContext is read-only and cannot save changes.");

    private static Task<int> ThrowReadOnlyAsync()
        => throw new InvalidOperationException("This DbContext is read-only and cannot save changes.");
}
