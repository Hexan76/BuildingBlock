using Framework.BuildingBlock.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Framework.BuildingBlock.EntityFrameworkCore;

/// <summary>
/// Entity Framework Core implementation of <see cref="IUnitOfWorkScope"/>.
/// The owning scope holds the <see cref="IDbContextTransaction"/> and is responsible for
/// commit/rollback; nested scopes are non-owning and defer to the outer one.
/// </summary>
public sealed class EfUnitOfWorkScope : IUnitOfWorkScope
{
    private readonly DbContext _context;
    private readonly IDbContextTransaction? _transaction;
    private readonly Action? _onFinished;
    private bool _finished;

    /// <summary>
    /// Creates a scope.
    /// </summary>
    /// <param name="context">The shared DbContext.</param>
    /// <param name="transaction">The owned transaction, or <c>null</c> for a nested (joining) scope.</param>
    /// <param name="onFinished">Callback invoked when the owning scope releases its transaction.</param>
    public EfUnitOfWorkScope(
        DbContext context,
        IDbContextTransaction? transaction,
        Action? onFinished)
    {
        _context = context;
        _transaction = transaction;
        _onFinished = onFinished;
    }

    public bool IsCompleted { get; private set; }

    public bool IsOwner => _transaction is not null;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        if (IsCompleted)
        {
            return;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Only the owning scope commits; nested scopes leave commit to the outer scope.
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(cancellationToken);
        }

        IsCompleted = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_finished)
        {
            return;
        }

        _finished = true;

        try
        {
            if (_transaction is not null)
            {
                if (!IsCompleted)
                {
                    await _transaction.RollbackAsync();
                }

                await _transaction.DisposeAsync();
            }
        }
        finally
        {
            _onFinished?.Invoke();
        }
    }

    public void Dispose()
        => DisposeAsync().AsTask().GetAwaiter().GetResult();
}
