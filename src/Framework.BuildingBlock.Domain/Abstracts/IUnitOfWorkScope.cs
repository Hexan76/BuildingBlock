namespace Framework.BuildingBlock.Data;

/// <summary>
/// A disposable transaction scope designed to be used with <c>using</c> / <c>await using</c>.
/// <para>
/// Opening a scope starts a database transaction. Work done through repositories that
/// share the same <c>DbContext</c> is applied only inside this transaction. The scope is
/// committed <b>only</b> when <see cref="CompleteAsync"/> is called; if the scope is
/// disposed without being completed, it is automatically rolled back.
/// </para>
/// <para>
/// Scopes can be nested: an inner scope opened while an outer scope (or an explicit
/// transaction) is already active simply joins it. Only the outermost, owning scope
/// actually commits or rolls back.
/// </para>
/// </summary>
/// <example>
/// <code>
/// await using (var scope = await unitOfWork.BeginScopeAsync(ct))
/// {
///     await repo.InsertAsync(entity, cancellationToken: ct);
///     await otherRepo.UpdateAsync(other, cancellationToken: ct);
///
///     await scope.CompleteAsync(ct); // commit; omit this to roll back
/// }
/// </code>
/// </example>
public interface IUnitOfWorkScope : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// True once <see cref="CompleteAsync"/> has been called successfully.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// True when this scope owns the underlying transaction (i.e. it is the outermost scope
    /// and is therefore responsible for commit/rollback).
    /// </summary>
    bool IsOwner { get; }

    /// <summary>
    /// Flushes pending changes to the database without ending the transaction.
    /// Useful when you need generated keys mid-scope.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves pending changes and, for the owning scope, commits the transaction.
    /// After this call the scope is considered completed and disposing it will not roll back.
    /// </summary>
    Task CompleteAsync(CancellationToken cancellationToken = default);
}
