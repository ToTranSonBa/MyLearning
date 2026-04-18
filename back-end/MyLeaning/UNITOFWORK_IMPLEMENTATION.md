# Unit of Work Implementation - Summary

## T?ng Quan Thay ??i

?ã implement Unit of Work pattern ?? centralize transaction management và repository access.

## L?i ích

? **Centralized Save Logic** - T?t c? SaveChanges ???c qu?n lý t? m?t n?i
? **Transaction Management** - Easy to use transactions v?i BeginTransaction, CommitTransaction, RollbackTransaction
? **Clean Code** - Gi?m code duplication, separation of concerns
? **Testable** - D? dàng mock IUnitOfWork trong unit tests
? **Consistency** - ??m b?o data consistency b?ng atomic operations

## C?u Trúc Th? M?c

```
Infrastructure.SqlServer/
??? UnitOfWork/
?   ??? UnitOfWork.cs              ? New
??? Repositories/
?   ??? BaseRepository.cs          ?? Modified (simplified)
?   ??? UserRepository.cs
??? DependencyInjection.cs         ?? Modified (added IUnitOfWork registration)

Application/
??? Common/Interfaces/
    ??? IUnitOfWork.cs             ? New
    ??? IGenericRepository.cs      ?? Modified (removed SaveChanges methods)
    ??? IUserRepository.cs
```

## Các Thay ??i Chi Ti?t

### 1. IGenericRepository Interface

**Tr??c:**
```csharp
public interface IGenericRepository<T> where T : class
{
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);       // ? Removed
    Task SaveChangesAsync();          // ? Removed
    void Update(T entity);
    void Delete(T entity);
}
```

**Sau:**
```csharp
public interface IGenericRepository<T> where T : class
{
    Task AddAsync(T entity);          // ? Just tracks changes
    void Update(T entity);            // ? Just tracks changes
    void Delete(T entity);            // ? Just tracks changes
    // SaveChanges handled by UnitOfWork
}
```

### 2. BaseRepository Class

**Tr??c:**
```csharp
public abstract class BaseRepository<T> : IGenericRepository<T> where T : class
{
    public async Task AddAsync(T entity)
    {
        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();  // ? Saves immediately
    }

    public async Task UpdateAsync(T entity)
    {
        _context.Update(entity);
        await _context.SaveChangesAsync();  // ? Saves immediately
    }
}
```

**Sau:**
```csharp
public abstract class BaseRepository<T> : IGenericRepository<T> where T : class
{
    public async Task AddAsync(T entity)
    {
        await _context.AddAsync(entity);    // ? Just tracks
    }

    public void Update(T entity)
    {
        _context.Update(entity);            // ? Just tracks
    }
}
```

### 3. IUnitOfWork Interface (New)

```csharp
public interface IUnitOfWork : IAsyncDisposable
{
    IUserRepository Users { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<bool> CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task<bool> RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

### 4. UnitOfWork Implementation (New)

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IUserRepository? _userRepository;
    private IDbContextTransaction? _transaction;
    
    public IUserRepository Users => _userRepository ??= new UserRepository(_context);
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
    
    // Transaction methods...
}
```

### 5. DependencyInjection Update

**Tr??c:**
```csharp
services.AddDbContext<ApplicationDbContext>(...);
services.AddScoped<IUserRepository, UserRepository>();  // ? Direct repository
```

**Sau:**
```csharp
services.AddDbContext<ApplicationDbContext>(...);
services.AddScoped<IUnitOfWork, UnitOfWork>();          // ? UnitOfWork
```

### 6. Auth Commands Update

T?t c? auth commands ?ã ???c c?p nh?t:

**Tr??c:**
```csharp
public class LoginHandler
{
    private readonly IUserRepository _userRepository;
    
    await _userRepository.UpdateAsync(user);        // ? Old way
    await _userRepository.SaveChangesAsync();
}
```

**Sau:**
```csharp
public class LoginHandler
{
    private readonly IUnitOfWork _unitOfWork;
    
    _unitOfWork.Users.Update(user);                 // ? New way
    await _unitOfWork.SaveChangesAsync(cancellationToken);
}
```

## Commands ???c C?p Nh?t

? RegisterCommand
? LoginCommand
? LogoutCommand
? ForgotPasswordCommand
? ResetPasswordCommand
? RefreshTokenCommand
? GetUserByIdQuery

## Cách Dùng

### Simple Update (Không Transaction)

```csharp
public async Task<bool> Handle(MyCommand request, CancellationToken cancellationToken)
{
    var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
    user.FullName = request.NewName;
    
    _unitOfWork.Users.Update(user);
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    
    return true;
}
```

### Complex Operation (V?i Transaction)

```csharp
public async Task<bool> Handle(ComplexCommand request, CancellationToken cancellationToken)
{
    var transactionStarted = await _unitOfWork.BeginTransactionAsync(cancellationToken);
    if (!transactionStarted)
        throw new Exception("Failed to begin transaction");
    
    try
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        user.FullName = request.NewName;
        _unitOfWork.Users.Update(user);
        
        // More operations...
        
        return await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
    catch (Exception)
    {
        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
        throw;
    }
}
```

## M? R?ng UnitOfWork

?? thêm repositories m?i:

1. **Update IUnitOfWork**
   ```csharp
   public interface IUnitOfWork : IAsyncDisposable
   {
       IUserRepository Users { get; }
       IProductRepository Products { get; }      // ? New
   }
   ```

2. **Update UnitOfWork Implementation**
   ```csharp
   public class UnitOfWork : IUnitOfWork
   {
       private IProductRepository? _productRepository;
       
       public IProductRepository Products => 
           _productRepository ??= new ProductRepository(_context);
   }
   ```

## Migration

?? apply database changes:

```bash
# If using Package Manager Console
Add-Migration AddAuthenticationFieldsUpdate
Update-Database

# Or using dotnet CLI
dotnet ef migrations add AddAuthenticationFieldsUpdate
dotnet ef database update
```

## Dependency Injection

```csharp
// Inject in handlers
public class MyHandler : IRequestHandler<MyCommand, MyResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    
    public MyHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}
```

## Best Practices

1. **Always use try-catch with transactions**
   ```csharp
   try
   {
       await _unitOfWork.BeginTransactionAsync();
       // operations...
       await _unitOfWork.CommitTransactionAsync();
   }
   catch
   {
       await _unitOfWork.RollbackTransactionAsync();
       throw;
   }
   ```

2. **Use cancellation tokens**
   ```csharp
   await _unitOfWork.SaveChangesAsync(cancellationToken);
   ```

3. **Keep transactions short**
   - Only begin transaction when necessary
   - Reduces lock time and improves concurrency

4. **Check return values**
   ```csharp
   var started = await _unitOfWork.BeginTransactionAsync();
   if (!started)
       throw new Exception("Transaction failed to start");
   ```

## Unit Testing

```csharp
[TestMethod]
public async Task LoginHandler_ShouldUpdateUserLastLogin()
{
    // Arrange
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var mockUser = new Users { Email = "test@test.com" };
    
    mockUnitOfWork
        .Setup(u => u.Users.GetByEmailAsync(It.IsAny<string>()))
        .ReturnsAsync(mockUser);
    
    mockUnitOfWork
        .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);
    
    var handler = new LoginHandler(mockUnitOfWork.Object);
    
    // Act
    var result = await handler.Handle(
        new LoginCommand("test@test.com", "password"),
        CancellationToken.None);
    
    // Assert
    mockUnitOfWork.Verify(
        u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
        Times.Once);
}
```

## Troubleshooting

### "The type or namespace name 'IUnitOfWork' could not be found"
- Ensure Application project references IUnitOfWork interface
- Check using statement: `using Application.Common.Interfaces;`

### Transaction not starting
```csharp
var started = await _unitOfWork.BeginTransactionAsync();
if (!started) 
    throw new Exception("Failed to begin transaction");
```

### Changes not saved
- Always call `await _unitOfWork.SaveChangesAsync()` after making changes
- Or use `CommitTransactionAsync()` if using transactions

## Tóm T?t

| Khía C?nh | Chi Ti?t |
|-----------|---------|
| **Pattern** | Unit of Work |
| **Main Interface** | `IUnitOfWork` |
| **Implementation** | `UnitOfWork` |
| **Repositories** | Accessed via `_unitOfWork.Users` |
| **Saving** | `await _unitOfWork.SaveChangesAsync()` |
| **Transactions** | `Begin/Commit/RollbackTransactionAsync()` |
| **Benefits** | Centralized, testable, clean code |
| **Testing** | Mock `IUnitOfWork` interface |

