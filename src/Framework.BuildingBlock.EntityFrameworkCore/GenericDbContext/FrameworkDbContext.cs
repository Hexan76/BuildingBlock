using System.Linq.Expressions;

using Framework.BuildingBlock.Abstracts;
using Framework.BuildingBlock.DependencyInjection;
using Framework.BuildingBlock.Domain;
using Framework.BuildingBlock.Entities;
using Framework.BuildingBlock.Extensions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.BuildingBlock.EntityFrameworkCore;

/// <summary>
/// A base <see cref="DbContext"/> that has no dependency on ABP.
/// It automatically:
/// <list type="bullet">
///   <item>fills audit properties (<see cref="IHasAuditProperties"/>),</item>
///   <item>converts hard deletes into soft deletes (<see cref="IHasSoftDelete"/>) and filters them out of queries,</item>
///   <item>manages optimistic concurrency stamps (<see cref="IHasConcurrencyStamp"/>),</item>
///   <item>normalizes <see cref="DateTime"/> values through <see cref="IClock"/>.</item>
/// </list>
/// Derive your application context from this class.
/// </summary>
public abstract class FrameworkDbContext : DbContext
{
    protected FrameworkDbContext(DbContextOptions options)
        : base(options)
    {
        // Instead of injecting IClock (and every other service) through the constructor -
        // which would bloat every derived context and break DbContext construction -
        // we grab the (scoped) application service provider that EF Core stores on the
        // options and expose it through a caching lazy provider. This is the same approach
        // ABP uses: the constructor stays clean and services are resolved on demand.
        LazyServiceProvider = ResolveLazyServiceProvider(options);
    }

    /// <summary>
    /// Lazy access to the DI container. Settable so advanced scenarios (e.g. pooled
    /// contexts) can refresh it per lease if needed.
    /// </summary>
    public ILazyServiceProvider LazyServiceProvider { get; set; }

    protected IClock Clock => LazyServiceProvider.GetRequiredService<IClock>();

    /// <summary>
    /// Timestamp source used for audit properties. Override to change the clock behavior.
    /// </summary>
    protected virtual DateTime Now => Clock.Now;

    /// <summary>
    /// Applied once by EF Core to <b>every</b> <see cref="DateTime"/>/<see cref="Nullable{DateTime}"/>
    /// property of every mapped entity, independent of the order in which derived contexts run
    /// their <see cref="OnModelCreating"/> logic. It forces all date/time values to be stored as UTC,
    /// which is required by Npgsql for <c>timestamp with time zone</c> columns and is immune to the
    /// fact that <see cref="DateTime"/> equality ignores <see cref="DateTime.Kind"/>.
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeValueConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcNullableDateTimeValueConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureFrameworkConventions(modelBuilder);
    }

    /// <summary>
    /// Applies the framework conventions (audit/concurrency/soft-delete mapping and the
    /// soft-delete query filter) to every mapped entity.
    /// Call <c>base.OnModelCreating</c> from derived contexts.
    /// </summary>
    protected virtual void ConfigureFrameworkConventions(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            modelBuilder.Entity(clrType).ConfigureByConvention();

            if (typeof(IHasSoftDelete).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType).HasQueryFilter(BuildSoftDeleteFilter(clrType));
            }
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyFrameworkConcepts();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyFrameworkConcepts();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private static ILazyServiceProvider ResolveLazyServiceProvider(DbContextOptions options)
    {
        var applicationServiceProvider = options
            .FindExtension<CoreOptionsExtension>()
            ?.ApplicationServiceProvider;

        // Reuse the scope's shared lazy provider when available; otherwise build a local one.
        return applicationServiceProvider?.GetService<ILazyServiceProvider>()
               ?? new LazyServiceProvider(applicationServiceProvider);
    }

    private void ApplyFrameworkConcepts()
    {
        var userId = CurrentUserContext.UserId;
        var now = Now;

        foreach (var entry in ChangeTracker.Entries())
        {
            NormalizeDateTimes(entry);

            switch (entry.State)
            {
                case EntityState.Added:
                    SetConcurrencyStampOnCreate(entry.Entity);
                    SetCreationAudit(entry.Entity, userId, now);
                    break;

                case EntityState.Modified:
                    RefreshConcurrencyStamp(entry.Entity);
                    SetModificationAudit(entry.Entity, userId, now);
                    break;

                case EntityState.Deleted:
                    HandleDelete(entry, userId, now);
                    break;
            }
        }
    }

    private void NormalizeDateTimes(EntityEntry entry)
    {
        if (entry.State is not (EntityState.Added or EntityState.Modified))
        {
            return;
        }

        foreach (var property in entry.Properties)
        {
            if (property.CurrentValue is DateTime dateTime)
            {
                property.CurrentValue = Clock.Normalize(dateTime);
            }
        }
    }

    private static void SetConcurrencyStampOnCreate(object entity)
    {
        if (entity is IHasConcurrencyStamp stamped && string.IsNullOrEmpty(stamped.ConcurrencyStamp))
        {
            stamped.ConcurrencyStamp = Guid.NewGuid().ToString("N");
        }
    }

    private static void RefreshConcurrencyStamp(object entity)
    {
        if (entity is IHasConcurrencyStamp stamped)
        {
            stamped.ConcurrencyStamp = Guid.NewGuid().ToString("N");
        }
    }

    private static void SetCreationAudit(object entity, Guid? userId, DateTime now)
    {
        if (entity is IHasAuditProperties audited)
        {
            if (audited.CreationTime == default)
            {
                audited.CreationTime = now;
            }

            audited.CreatorId ??= userId;
        }
    }

    private static void SetModificationAudit(object entity, Guid? userId, DateTime now)
    {
        if (entity is IHasAuditProperties audited)
        {
            audited.ModificationTime = now;
            audited.ModifierId = userId;
        }
    }

    private void HandleDelete(EntityEntry entry, Guid? userId, DateTime now)
    {
        if (entry.Entity is not IHasSoftDelete softDelete)
        {
            return;
        }

        entry.State = EntityState.Modified;
        softDelete.IsDeleted = true;
        softDelete.DeletionTime = now;
        softDelete.DeleterId = userId;

        RefreshConcurrencyStamp(entry.Entity);
        SetModificationAudit(entry.Entity, userId, now);
    }

    private static LambdaExpression BuildSoftDeleteFilter(Type clrType)
    {
        // Builds: entity => !((IHasSoftDelete)entity).IsDeleted
        var parameter = Expression.Parameter(clrType, "e");
        var isDeletedProperty = Expression.Property(
            Expression.Convert(parameter, typeof(IHasSoftDelete)),
            nameof(IHasSoftDelete.IsDeleted));
        var body = Expression.Not(isDeletedProperty);

        return Expression.Lambda(body, parameter);
    }
}
