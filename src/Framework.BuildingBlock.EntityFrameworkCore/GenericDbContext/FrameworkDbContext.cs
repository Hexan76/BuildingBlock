using System.Linq.Expressions;

using Framework.BuildingBlock.Data;
using Framework.BuildingBlock.Entities;

using Microsoft.EntityFrameworkCore;

namespace Framework.BuildingBlock.EntityFrameworkCore;

/// <summary>
/// A base <see cref="DbContext"/> that has no dependency on ABP.
/// It automatically:
/// <list type="bullet">
///   <item>fills audit properties (<see cref="IHasAuditProperties"/>),</item>
///   <item>converts hard deletes into soft deletes (<see cref="IHasSoftDelete"/>) and filters them out of queries,</item>
///   <item>manages optimistic concurrency stamps (<see cref="IHasConcurrencyStamp"/>).</item>
/// </list>
/// Derive your application context from this class.
/// </summary>
public abstract class FrameworkDbContext : DbContext
{
    private readonly ICurrentUserAccessor _currentUser;

    protected FrameworkDbContext(
        DbContextOptions options,
        ICurrentUserAccessor? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser ?? NullCurrentUserAccessor.Instance;
    }

    /// <summary>
    /// Timestamp source used for audit properties. Override to change the clock (e.g. local time).
    /// </summary>
    protected virtual DateTime Now => DateTime.UtcNow;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureFrameworkConventions(modelBuilder);
    }

    /// <summary>
    /// Applies the framework conventions (concurrency token + soft-delete query filter)
    /// to every mapped entity. Call <c>base.OnModelCreating</c> from derived contexts.
    /// </summary>
    protected virtual void ConfigureFrameworkConventions(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (typeof(IHasConcurrencyStamp).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType)
                    .Property(nameof(IHasConcurrencyStamp.ConcurrencyStamp))
                    .IsConcurrencyToken()
                    .HasMaxLength(40);
            }

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

    private void ApplyFrameworkConcepts()
    {
        var userId = _currentUser.UserId;
        var now = Now;

        foreach (var entry in ChangeTracker.Entries())
        {
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

    private void HandleDelete(
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry,
        Guid? userId,
        DateTime now)
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
