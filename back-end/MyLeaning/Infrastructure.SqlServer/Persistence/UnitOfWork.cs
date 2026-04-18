using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.SqlServer.Persistence;

/// <summary>
/// Unit of Work implementation - Manages transactions and persistence ONLY.
/// 
/// This implementation:
/// - Does NOT expose repositories
/// - Only handles SaveChanges and transaction management
/// - Works with DbContext to track entity changes
/// 
/// Separation of Concerns:
/// - Repositories are injected separately into services
/// - UnitOfWork only coordinates persistence and transactions
/// - Clean and focused responsibility
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return _transaction != null;
    }

    public async Task<bool> CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync();
            await _transaction?.CommitAsync()!;
            return true;
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task<bool> RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
            return true;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
