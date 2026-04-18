# Unit of Work Pattern Documentation

## Gi?i thi?u

Unit of Work pattern là m?t m?u thi?t k? ?? qu?n lý transactions và coordinating multiple repositories. Nó cung c?p m?t interface unified ?? truy c?p t?t c? repositories và qu?n lý vi?c l?u tr? d? li?u.

## L?i ích

1. **Transaction Management**: Qu?n lý transactions m?t cách t?p trung
2. **Consistency**: ??m b?o d? li?u consistency b?ng cách commit/rollback t?t c? changes cùng lúc
3. **Clean Code**: Gi?m code duplication, single responsibility
4. **Easy Testing**: D? dàng mock IUnitOfWork trong unit tests
5. **Centralized Save Logic**: T?t c? SaveChanges ???c qu?n lý t? m?t n?i

## C?u trúc

### IUnitOfWork Interface

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

### Các Method Chính

| Method | Mô t? |
|--------|--------|
| `Users` | Property ?? truy c?p UserRepository |
| `SaveChangesAsync()` | L?u t?t c? tracked changes vào database |
| `BeginTransactionAsync()` | B?t ??u m?t database transaction |
| `CommitTransactionAsync()` | Commit transaction và l?u changes |
| `RollbackTransactionAsync()` | Rollback transaction, discard changes |

## Cách S? D?ng

### 1. Simple Usage (Không dùng Transaction)

```csharp
public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
{
    // Get user
    var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
    
    // Make changes
    user.LastLoginAt = DateTime.UtcNow;
    user.RefreshToken = refreshToken;
    
    // Track changes
    _unitOfWork.Users.Update(user);
    
    // Save changes
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    
    return new AuthResponseDto(...);
}
```

### 2. With Transaction

```csharp
public async Task<bool> Handle(SomeCommand request, CancellationToken cancellationToken)
{
    // Begin transaction
    var transactionStarted = await _unitOfWork.BeginTransactionAsync(cancellationToken);
    if (!transactionStarted)
        throw new Exception("Failed to begin transaction");
    
    try
    {
        // Get user
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        
        // Make multiple changes
        user.FullName = request.NewName;
        _unitOfWork.Users.Update(user);
        
        // Make more changes to other entities
        // await _unitOfWork.OtherRepository.AddAsync(someEntity);
        
        // Commit transaction (saves all changes atomically)
        var committed = await _unitOfWork.CommitTransactionAsync(cancellationToken);
        return committed;
    }
    catch (Exception ex)
    {
        // Rollback on any error
        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
        throw;
    }
}
```

### 3. Multiple Repository Operations

```csharp
public async Task<bool> Handle(TransferCommand request, CancellationToken cancellationToken)
{
    await _unitOfWork.BeginTransactionAsync(cancellationToken);
    
    try
    {
        // Get sender user
        var sender = await _unitOfWork.Users.GetByGuidAsync(request.SenderId);
        sender.Balance -= request.Amount;
        _unitOfWork.Users.Update(sender);
        
        // Get receiver user
        var receiver = await _unitOfWork.Users.GetByGuidAsync(request.ReceiverId);
        receiver.Balance += request.Amount;
        _unitOfWork.Users.Update(receiver);
        
        // Commit when all operations succeed
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
        return true;
    }
    catch (Exception)
    {
        // Rollback if any operation fails
        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
        throw;
    }
}
```

## Dependency Injection

UnitOfWork ???c register t?i kh?i ??ng application:

```csharp
// Infrastructure.SqlServer/DependencyInjection.cs
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

Inject vào handlers:

```csharp
public class LoginHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    
    public LoginHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}
```

## Thay ??i t? Cách C?

### Cách C? (Direct Repository)

```csharp
public class LoginHandler
{
    private readonly IUserRepository _userRepository;
    
    public async Task<bool> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        user.LastLoginAt = DateTime.UtcNow;
        
        await _userRepository.UpdateAsync(user);      // ? Saves immediately
        await _userRepository.SaveChangesAsync();      // ? Redundant
    }
}
```

### Cách M?i (UnitOfWork)

```csharp
public class LoginHandler
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<bool> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        user.LastLoginAt = DateTime.UtcNow;
        
        _unitOfWork.Users.Update(user);                  // ? Tracks changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);  // ? Explicit save
    }
}
```

## Khi Nào Dùng Transaction

### Dùng Transaction Khi:
- ? Multiple operations ph?i thành công cùng lúc
- ? Có dependency gi?a các operations
- ? C?n ??m b?o data consistency
- ? Có risk c?a concurrent modifications

### Không C?n Transaction Khi:
- ? Single operation (update 1 user)
- ? Read-only queries
- ? Independent operations

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

3. **Check transaction start result**
   ```csharp
   var started = await _unitOfWork.BeginTransactionAsync();
   if (!started) throw new Exception("Failed to begin transaction");
   ```

4. **Keep transactions short**
   - Ch? gi? transaction khi th?c s? c?n
   - Gi?m lock time, improve concurrency

5. **Use Using statement for proper disposal**
   ```csharp
   using (var unitOfWork = new UnitOfWork(context))
   {
       // operations...
   } // Automatically disposed
   ```

## Testing

### Unit Test Example

```csharp
[TestClass]
public class LoginHandlerTests
{
    [TestMethod]
    public async Task Handle_ShouldUpdateUserLastLogin()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockUser = new Users { Id = Guid.NewGuid(), Email = "test@test.com" };
        
        mockUnitOfWork
            .Setup(u => u.Users.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(mockUser);
        
        mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        
        var handler = new LoginHandler(mockUnitOfWork.Object);
        
        // Act
        var result = await handler.Handle(new LoginCommand("test@test.com", "password"), CancellationToken.None);
        
        // Assert
        mockUnitOfWork.Verify(u => u.Users.Update(It.IsAny<Users>()), Times.Once);
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

## M? R?ng UnitOfWork

Thêm repositories m?i:

```csharp
public interface IUnitOfWork : IAsyncDisposable
{
    IUserRepository Users { get; }
    IProductRepository Products { get; }      // ? New
    IOrderRepository Orders { get; }          // ? New
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    // ... transaction methods
}

public class UnitOfWork : IUnitOfWork
{
    private IProductRepository? _productRepository;
    private IOrderRepository? _orderRepository;
    
    public IProductRepository Products => _productRepository ??= new ProductRepository(_context);
    public IOrderRepository Orders => _orderRepository ??= new OrderRepository(_context);
    
    // ... rest of implementation
}
```

## Tóm T?t

| Khía C?nh | Chi Ti?t |
|-----------|---------|
| **M?c ?ích** | Centralized transaction & repository management |
| **Main Interface** | `IUnitOfWork` |
| **Implementation** | `UnitOfWork` |
| **Usage** | Inject `IUnitOfWork`, use `.Users.X()` |
| **Transaction** | `Begin ? Operations ? Commit/Rollback` |
| **Save Method** | `SaveChangesAsync()` |
| **Benefit** | Clean, consistent, testable code |

