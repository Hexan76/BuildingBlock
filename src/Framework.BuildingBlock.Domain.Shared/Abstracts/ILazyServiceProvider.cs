namespace Framework.BuildingBlock.Abstracts;

/// <summary>
/// A thin, caching wrapper around a (scoped) <see cref="IServiceProvider"/>.
/// <para>
/// It lets long-lived or infrastructure objects (such as a <c>DbContext</c>) obtain
/// framework services <b>lazily</b> and <b>on demand</b> instead of receiving them all
/// through the constructor. This is the same idea ABP uses to keep the DbContext
/// constructor clean and pooling-friendly while still giving access to services like
/// <c>IClock</c>, current-user accessors, event publishers, etc.
/// </para>
/// </summary>
public interface ILazyServiceProvider
{
    /// <summary>
    /// Resolves a required service, throwing if it is not registered.
    /// </summary>
    T GetRequiredService<T>();

    /// <summary>
    /// Resolves a required service, throwing if it is not registered.
    /// </summary>
    object GetRequiredService(Type serviceType);

    /// <summary>
    /// Resolves an optional service, returning <c>null</c> when it is not registered.
    /// </summary>
    T? GetService<T>();

    /// <summary>
    /// Resolves an optional service, returning <c>null</c> when it is not registered.
    /// </summary>
    object? GetService(Type serviceType);
}
