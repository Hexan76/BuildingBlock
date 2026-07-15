namespace Framework.BuildingBlock.Data;

/// <summary>
/// Coordinates saving changes and database transactions across repositories that
/// share the same <c>DbContext</c>. Fully independent of ABP's Unit of Work.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// True when an explicit transaction is currently active.
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// Persists all pending changes to the store.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a disposable transaction <see cref="IUnitOfWorkScope"/> intended to be consumed
    /// with <c>using</c> / <c>await using</c>. The scope commits only when
    /// <see cref="IUnitOfWorkScope.CompleteAsync"/> is called and otherwise rolls back on dispose.
    /// If a transaction is already active, the returned scope joins it (nested, non-owning).
    /// </summary>
    Task<IUnitOfWorkScope> BeginScopeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves pending changes (if any) and commits the active transaction.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs <paramref name="operation"/> inside a transaction, committing on success
    /// and rolling back if an exception is thrown. Nested calls join the outer transaction.
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);

    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);
}
