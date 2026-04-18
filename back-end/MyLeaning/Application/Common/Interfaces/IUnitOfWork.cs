namespace Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern - Manages transactions and persistence.
/// Responsibility: ONLY SaveChanges and Transaction management
/// 
/// It does NOT contain repository references!
/// Repositories are injected separately into services/handlers.
/// 
/// This keeps the interface clean and focused on its single responsibility.
/// 
/// Design Principle:
/// - IUnitOfWork = Transaction coordinator + Persistence layer
/// - IUserRepository = Data query interface
/// - They are SEPARATE concerns, injected independently
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>Save all tracked changes to database</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>Begin a new database transaction</summary>
    Task<bool> BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>Commit current transaction (saves changes)</summary>
    Task<bool> CommitTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>Rollback current transaction (discards changes)</summary>
    Task<bool> RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
