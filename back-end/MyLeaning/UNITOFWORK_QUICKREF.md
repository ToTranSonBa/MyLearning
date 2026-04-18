# Unit of Work - Quick Reference

## ? Quick Start

### Inject UnitOfWork

```csharp
public class MyHandler : IRequestHandler<MyCommand, MyResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    
    public MyHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}
```

### Simple Usage (No Transaction)

```csharp
public async Task<bool> Handle(MyCommand request, CancellationToken cancellationToken)
{
    // Get
    var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
    
    // Modify
    user.LastName = request.NewName;
    
    // Update
    _unitOfWork.Users.Update(user);
    
    // Save
    await _unitOfWork.SaveChangesAsync(cancellationToken);
    
    return true;
}
```

### With Transaction

```csharp
var transactionStarted = await _unitOfWork.BeginTransactionAsync(cancellationToken);
if (!transactionStarted) throw new Exception("Transaction failed");

try
{
    var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
    user.LastName = request.NewName;
    _unitOfWork.Users.Update(user);
    
    return await _unitOfWork.CommitTransactionAsync(cancellationToken);
}
catch (Exception)
{
    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
    throw;
}
```

## ?? Available Methods

| Method | Purpose | Returns |
|--------|---------|---------|
| `_unitOfWork.Users` | Access UserRepository | `IUserRepository` |
| `GetByEmailAsync(email)` | Find user by email | `Users?` |
| `GetByUsernameAsync(username)` | Find user by username | `Users?` |
| `GetByGuidAsync(id)` | Find user by ID | `Users?` |
| `GetAllAsync()` | Get all users | `IEnumerable<Users>` |
| `AddAsync(entity)` | Add new user | `Task` |
| `Update(entity)` | Update existing user | `void` |
| `Delete(entity)` | Delete user | `void` |
| `SaveChangesAsync()` | Save all tracked changes | `Task<int>` |
| `BeginTransactionAsync()` | Start transaction | `Task<bool>` |
| `CommitTransactionAsync()` | Commit transaction | `Task<bool>` |
| `RollbackTransactionAsync()` | Rollback transaction | `Task<bool>` |

## ?? Common Patterns

### 1. Create & Save
```csharp
var newUser = new Users 
{ 
    UserName = "john", 
    Email = "john@test.com",
    FullName = "John Doe"
};
await _unitOfWork.Users.AddAsync(newUser);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

### 2. Update & Save
```csharp
var user = await _unitOfWork.Users.GetByEmailAsync("john@test.com");
user.FullName = "Jane Doe";
_unitOfWork.Users.Update(user);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

### 3. Delete & Save
```csharp
var user = await _unitOfWork.Users.GetByEmailAsync("john@test.com");
_unitOfWork.Users.Delete(user);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

### 4. Multiple Operations (Transaction)
```csharp
await _unitOfWork.BeginTransactionAsync(cancellationToken);
try
{
    // Operation 1
    var user1 = await _unitOfWork.Users.GetByEmailAsync("user1@test.com");
    user1.FullName = "Updated User 1";
    _unitOfWork.Users.Update(user1);
    
    // Operation 2
    var user2 = await _unitOfWork.Users.GetByEmailAsync("user2@test.com");
    user2.FullName = "Updated User 2";
    _unitOfWork.Users.Update(user2);
    
    // Commit both
    await _unitOfWork.CommitTransactionAsync(cancellationToken);
}
catch (Exception)
{
    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
    throw;
}
```

## ? What NOT to Do

```csharp
// ? DON'T - These methods don't exist anymore
_unitOfWork.Users.UpdateAsync(user);
_unitOfWork.Users.SaveChangesAsync();

// ? DO - Use Update + SaveChangesAsync
_unitOfWork.Users.Update(user);
await _unitOfWork.SaveChangesAsync(cancellationToken);

// ? DON'T - Don't forget to call SaveChangesAsync
user.Name = "New Name";
_unitOfWork.Users.Update(user);
// Changes are NOT saved without this:
// await _unitOfWork.SaveChangesAsync(cancellationToken);

// ? DO - Always save changes
user.Name = "New Name";
_unitOfWork.Users.Update(user);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

## ?? Current Implementation

### IUnitOfWork Interface Location
`Application/Common/Interfaces/IUnitOfWork.cs`

### UnitOfWork Implementation Location
`Infrastructure.SqlServer/UnitOfWork/UnitOfWork.cs`

### Dependency Injection
Registered in `Infrastructure.SqlServer/DependencyInjection.cs`:
```csharp
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

## ?? Testing

```csharp
var mockUnitOfWork = new Mock<IUnitOfWork>();

// Setup
mockUnitOfWork
    .Setup(u => u.Users.GetByEmailAsync(It.IsAny<string>()))
    .ReturnsAsync(new Users { Email = "test@test.com" });

mockUnitOfWork
    .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(1);

// Use
var handler = new MyHandler(mockUnitOfWork.Object);
var result = await handler.Handle(new MyCommand(...), CancellationToken.None);

// Verify
mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
```

## ?? Common Issues

### Issue: "Transaction failed to start"
```csharp
var started = await _unitOfWork.BeginTransactionAsync(cancellationToken);
if (!started)
    throw new Exception("Failed to begin transaction");
```

### Issue: "Changes not saved"
Make sure you call `SaveChangesAsync()`:
```csharp
_unitOfWork.Users.Update(user);
await _unitOfWork.SaveChangesAsync(cancellationToken);  // ? Don't forget!
```

### Issue: "IUnitOfWork not found"
Check using statements:
```csharp
using Application.Common.Interfaces;  // ? Add this
```

## ?? Related Guides
- [Full Unit of Work Guide](UNITOFWORK_GUIDE.md)
- [Implementation Details](UNITOFWORK_IMPLEMENTATION.md)
- [JWT Authentication](JWT_AUTHENTICATION_SETUP.md)

