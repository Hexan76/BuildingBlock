using Framework.BuildingBlock.Data;
using Framework.BuildingBlock.DependencyInjection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Framework.BuildingBlock.EntityFrameworkCore;

public static class FrameworkDataServiceCollectionExtensions
{
    /// <summary>
    /// Registers the ABP-free data stack (generic repository + unit of work) for the
    /// given <typeparamref name="TDbContext"/>.
    /// <para>
    /// The <c>DbContext</c> itself must already be registered (e.g. via
    /// <c>services.AddDbContext&lt;TDbContext&gt;(...)</c>). This method exposes it as the
    /// base <see cref="DbContext"/> so the open-generic repository and the unit of work
    /// can resolve it, and registers a no-op <see cref="ICurrentUserAccessor"/> unless the
    /// host application provides its own.
    /// </para>
    /// </summary>
    public static IServiceCollection AddFrameworkData<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.TryAddScoped<DbContext>(sp => sp.GetRequiredService<TDbContext>());

        // IClock + ILazyServiceProvider used by FrameworkDbContext.
        services.AddFrameworkClock();

        services.TryAddSingleton<ICurrentUserAccessor>(NullCurrentUserAccessor.Instance);

        services.TryAddScoped(typeof(IGenericRepository<,>), typeof(EfGenericRepository<,>));
        services.TryAddScoped(typeof(IGenericRepository<>), typeof(EfGenericRepository<>));

        services.TryAddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}
