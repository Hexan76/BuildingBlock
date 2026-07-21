using Framework.BuildingBlock.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Framework.BuildingBlock.EntityFrameworkCore;

/// <summary>
/// Entity Framework Core implementation of <see cref="IUnitOfWork"/>.
/// Wraps a single <see cref="DbContext"/> and its ambient database transaction.
/// Registered as scoped so every repository sharing the same context participates
/// in the same transaction.
/// </summary>
public class EfUnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public EfUnitOfWork(DbContext context)
    {
        _context = context;
    }

    public bool HasActiveTransaction => _transaction is not null;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            return;
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task<IUnitOfWorkScope> BeginScopeAsync(CancellationToken cancellationToken = default)
    {
        // A transaction is already active (an outer scope or an explicit BeginTransactionAsync):
        // return a nested, non-owning scope that joins it.
        if (_transaction is not null)
        {
            return new EfUnitOfWorkScope(_context, transaction: null, onFinished: null);
        }

        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        _transaction = transaction;

        return new EfUnitOfWorkScope(_context, transaction, onFinished: () => _transaction = null);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);

        if (_transaction is null)
        {
            return;
        }

        try
        {
            await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            return;
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        // Join an already-open transaction instead of nesting.
        if (_transaction is not null)
        {
            return await operation(cancellationToken);
        }

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await operation(cancellationToken);
                await CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
        => ExecuteInTransactionAsync<object?>(async ct =>
        {
            await operation(ct);
            return null;
        }, cancellationToken);

    private async Task DisposeTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _transaction?.Dispose();
        _transaction = null;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await DisposeTransactionAsync();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
