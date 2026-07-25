using System.Collections.Concurrent;

using Framework.BuildingBlock.Abstracts;

using Microsoft.Extensions.DependencyInjection;

namespace Framework.BuildingBlock.DependencyInjection;

/// <summary>
/// Default <see cref="ILazyServiceProvider"/> implementation. It wraps the current
/// (scoped) <see cref="IServiceProvider"/> and caches resolved services for the lifetime
/// of the wrapper so repeated lookups (e.g. inside a DbContext) are cheap.
/// </summary>
public sealed class LazyServiceProvider : ILazyServiceProvider
{
    private readonly IServiceProvider? _serviceProvider;
    private readonly ConcurrentDictionary<Type, object?> _cachedServices = new();

    public LazyServiceProvider(IServiceProvider? serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T GetRequiredService<T>()
        => (T)GetRequiredService(typeof(T));

    public object GetRequiredService(Type serviceType)
    {
        EnsureProviderAvailable();

        return _cachedServices.GetOrAdd(
            serviceType,
            static (type, provider) => provider.GetRequiredService(type),
            _serviceProvider!)!;
    }

    public T? GetService<T>()
        => (T?)GetService(typeof(T));

    public object? GetService(Type serviceType)
    {
        EnsureProviderAvailable();

        if (_cachedServices.TryGetValue(serviceType, out var cached))
        {
            return cached;
        }

        var service = _serviceProvider!.GetService(serviceType);
        if (service is not null)
        {
            _cachedServices.TryAdd(serviceType, service);
        }

        return service;
    }

    private void EnsureProviderAvailable()
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException(
                "No service provider is available on this LazyServiceProvider. " +
                "This typically happens when the owning object (e.g. a DbContext) was created " +
                "outside of the DI container - such as at design time - and then requested a " +
                "framework service. Resolve the object from the DI container so a scoped " +
                "service provider is available.");
        }
    }
}
