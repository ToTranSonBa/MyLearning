# Unit of Work & Authentication - Step by Step Guide

## ?? M?c Tiêu
H??ng d?n t?ng b??c ?? s? d?ng Unit of Work pattern và authentication system.

---

## ?? M?c L?c
1. [B??c 1: Hi?u UnitOfWork](#b??c-1-hi?u-unitofwork)
2. [B??c 2: T?o Handler v?i UnitOfWork](#b??c-2-t?o-handler-v?i-unitofwork)
3. [B??c 3: Th?c hi?n Transactions](#b??c-3-th?c-hi?n-transactions)
4. [B??c 4: Testing](#b??c-4-testing)
5. [B??c 5: Real World Examples](#b??c-5-real-world-examples)

---

## B??c 1: Hi?u UnitOfWork

### 1.1 Dependency Injection

Khi ?ng d?ng kh?i ??ng, UnitOfWork ???c register:

```csharp
// Infrastructure.SqlServer/DependencyInjection.cs
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### 1.2 UnitOfWork Là Gì?

UnitOfWork là m?t wrapper quanh DbContext:

```
???????????????????????????
?    IUnitOfWork          ?
???????????????????????????
? - Users (IUserRepo)     ?
? - Products (IProductRepo) ? Future
? - SaveChangesAsync()    ?
? - BeginTransaction()    ?
? - CommitTransaction()   ?
? - RollbackTransaction() ?
???????????????????????????
           ?
           ?
???????????????????????????
?  ApplicationDbContext   ?
?  (Entity Framework)     ?
???????????????????????????
           ?
           ?
???????????????????????????
?  SQL Server Database    ?
???????????????????????????
```

### 1.3 Key Concepts

| Concept | Meaning |
|---------|---------|
| **Tracking** | EF Core theo dõi thay ??i trên object |
| **SaveChanges** | Ghi t?t c? tracked changes vào DB |
| **Transaction** | Nhóm multiple operations thành m?t unit |
| **Commit** | L?u transaction vào DB |
| **Rollback** | H?y transaction, restore original state |

---

## B??c 2: T?o Handler v?i UnitOfWork

### 2.1 Basic Handler Structure

```csharp
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Users.Commands
{
    public record UpdateUserCommand(Guid UserId, string NewName) : IRequest<bool>;

    public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        // ? Inject IUnitOfWork
        public UpdateUserHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(
            UpdateUserCommand request, 
            CancellationToken cancellationToken)
        {
            // 1. Get user
            var user = await _unitOfWork.Users.GetByGuidAsync(request.UserId);
            if (user == null)
                return false;

            // 2. Make changes
            user.FullName = request.NewName;

            // 3. Track changes
            _unitOfWork.Users.Update(user);

            // 4. Save to database
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
```

### 2.2 Step-by-Step Breakdown

```csharp
// Step 1: Get Data
var user = await _unitOfWork.Users.GetByEmailAsync("john@test.com");
// State: ? Object loaded, ? EF tracking enabled

// Step 2: Modify
user.FullName = "Jane Doe";
// State: ? Object modified, ? Not saved yet

// Step 3: Tell EF to track changes
_unitOfWork.Users.Update(user);
// State: ? EF knows about changes

// Step 4: Save to database
await _unitOfWork.SaveChangesAsync(cancellationToken);
// State: ? Changes persisted to DB
```

### 2.3 Common Repository Methods

```csharp
// Get
var user = await _unitOfWork.Users.GetByEmailAsync("test@test.com");
var user = await _unitOfWork.Users.GetByGuidAsync(userId);
var user = await _unitOfWork.Users.GetByUsernameAsync("john_doe");
var users = await _unitOfWork.Users.GetAllAsync();

// Add
await _unitOfWork.Users.AddAsync(newUser);

// Update
_unitOfWork.Users.Update(existingUser);

// Delete
_unitOfWork.Users.Delete(userToDelete);

// Save (required after Add/Update/Delete)
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

---

## B??c 3: Th?c hi?n Transactions

### 3.1 Khi Nào C?n Transaction

**Dùng Transaction khi:**
- ? Multiple operations ph?i thành công cùng lúc
- ? Có dependency gi?a operations
- ? C?n ??m b?o data consistency
- ? Transfer ti?n, order processing, etc.

**Không c?n Transaction khi:**
- ? Single operation (update 1 user)
- ? Read-only queries
- ? Independent operations

### 3.2 Basic Transaction Example

```csharp
public async Task<bool> Handle(
    SomeCommand request, 
    CancellationToken cancellationToken)
{
    // ? Begin transaction
    var transactionStarted = await _unitOfWork.BeginTransactionAsync(cancellationToken);
    if (!transactionStarted)
        throw new Exception("Failed to begin transaction");

    try
    {
        // ? Do operations
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        user.FullName = request.NewName;
        _unitOfWork.Users.Update(user);
        
        // ? Commit transaction (saves all changes)
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
        return true;
    }
    catch (Exception)
    {
        // ? Rollback on error
        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
        throw;
    }
}
```

### 3.3 Real World: Bank Transfer

```csharp
public async Task<bool> TransferMoney(
    Guid senderId, 
    Guid receiverId, 
    decimal amount,
    CancellationToken cancellationToken)
{
    // ? Start transaction
    var started = await _unitOfWork.BeginTransactionAsync(cancellationToken);
    if (!started)
        throw new Exception("Transaction failed to start");

    try
    {
        // ? Get sender
        var sender = await _unitOfWork.Users.GetByGuidAsync(senderId);
        if (sender.Balance < amount)
            throw new Exception("Insufficient funds");

        // ? Get receiver
        var receiver = await _unitOfWork.Users.GetByGuidAsync(receiverId);

        // ? Deduct from sender
        sender.Balance -= amount;
        _unitOfWork.Users.Update(sender);

        // ? Add to receiver
        receiver.Balance += amount;
        _unitOfWork.Users.Update(receiver);

        // ? If everything succeeds, commit
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
        return true;
    }
    catch (Exception ex)
    {
        // ? If anything fails, rollback
        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
        throw;
    }
}
```

### 3.4 Transaction Flow Diagram

```
Try Block
?? BeginTransaction()
?  ?? Lock resources
?? Operation 1 (Success ?)
?? Operation 2 (Success ?)
?? Operation 3 (Success ?)
?? CommitTransaction()
?  ?? Save ALL changes at once
?? Return True

If ANY exception:
?? RollbackTransaction()
?  ?? Discard ALL changes
?? Throw exception
```

---

## B??c 4: Testing

### 4.1 Unit Test Setup

```csharp
using Moq;
using Xunit;
using Application.Features.Users.Commands;
using Application.Common.Interfaces;

public class UpdateUserHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateUserName()
    {
        // Arrange: Setup mocks
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var userId = Guid.NewGuid();
        var existingUser = new Users 
        { 
            Id = userId,
            FullName = "John Doe"
        };

        // ? Mock GetByGuidAsync
        mockUnitOfWork
            .Setup(u => u.Users.GetByGuidAsync(userId))
            .ReturnsAsync(existingUser);

        // ? Mock SaveChangesAsync
        mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UpdateUserHandler(mockUnitOfWork.Object);

        // Act: Execute handler
        var result = await handler.Handle(
            new UpdateUserCommand(userId, "Jane Doe"),
            CancellationToken.None);

        // Assert: Verify results
        Assert.True(result);
        
        // ? Verify Update was called
        mockUnitOfWork.Verify(
            u => u.Users.Update(It.IsAny<Users>()), 
            Times.Once);

        // ? Verify SaveChangesAsync was called
        mockUnitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), 
            Times.Once);
    }
}
```

### 4.2 Integration Test Setup

```csharp
public class UpdateUserIntegrationTests
{
    [Fact]
    public async Task Handle_ShouldPersistChangesToDatabase()
    {
        // Arrange: Use real database context
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            var unitOfWork = new UnitOfWork(context);
            var handler = new UpdateUserHandler(unitOfWork);
            
            // Create test user
            var user = new Users 
            { 
                UserName = "testuser",
                Email = "test@test.com",
                FullName = "John Doe"
            };
            await unitOfWork.Users.AddAsync(user);
            await unitOfWork.SaveChangesAsync();

            // Act: Update user
            var result = await handler.Handle(
                new UpdateUserCommand(user.Id, "Jane Doe"),
                CancellationToken.None);

            // Assert: Verify in database
            Assert.True(result);
            var updatedUser = await unitOfWork.Users.GetByGuidAsync(user.Id);
            Assert.Equal("Jane Doe", updatedUser.FullName);
        }
    }
}
```

---

## B??c 5: Real World Examples

### 5.1 User Registration (Auth System)

```csharp
public async Task<AuthResponseDto> Handle(
    RegisterCommand request, 
    CancellationToken cancellationToken)
{
    // ? Check if user exists
    var existing = await _unitOfWork.Users.GetByEmailAsync(request.Email);
    if (existing != null)
        throw new BadRequestException("Email already registered");

    // ? Create new user
    var user = new Users
    {
        UserName = request.UserName,
        Email = request.Email,
        FullName = request.FullName,
        PasswordHash = _passwordHasher.HashPassword(request.Password),
        CreatedAt = DateTime.UtcNow,
        IsEmailConfirmed = false
    };

    // ? Add and save
    await _unitOfWork.Users.AddAsync(user);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    // ? Generate tokens
    var accessToken = _tokenService.GenerateAccessToken(user);
    var refreshToken = _tokenService.GenerateRefreshToken();

    // ? Update with tokens
    user.RefreshToken = refreshToken;
    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
    _unitOfWork.Users.Update(user);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return new AuthResponseDto(
        accessToken, 
        refreshToken, 
        user.Id, 
        user.UserName, 
        user.Email);
}
```

### 5.2 Password Reset

```csharp
public async Task<string> Handle(
    ResetPasswordCommand request, 
    CancellationToken cancellationToken)
{
    // ? Find user by reset token
    var user = await _unitOfWork.Users.GetByPasswordResetTokenAsync(request.Token);
    if (user == null)
        throw new UnauthorizedException("Invalid or expired reset token");

    // ? Check token expiry
    if (user.PasswordResetTokenExpiryTime < DateTime.UtcNow)
        throw new UnauthorizedException("Password reset token has expired");

    // ? Update password
    user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
    user.PasswordResetToken = null;
    user.PasswordResetTokenExpiryTime = null;

    // ? Save changes
    _unitOfWork.Users.Update(user);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return "Password has been reset successfully";
}
```

### 5.3 Logout

```csharp
public async Task<Unit> Handle(
    LogoutCommand request, 
    CancellationToken cancellationToken)
{
    // ? Get user
    var user = await _unitOfWork.Users.GetByGuidAsync(request.UserId);
    if (user == null)
        throw new NotFoundException("User not found");

    // ? Invalidate refresh token
    user.RefreshToken = null;
    user.RefreshTokenExpiryTime = null;

    // ? Save changes
    _unitOfWork.Users.Update(user);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Unit.Value;
}
```

---

## ?? Checklist: T?o Handler M?i

Khi t?o handler m?i, follow checklist này:

- [ ] Inject `IUnitOfWork` thay vì `IUserRepository`
- [ ] G?i `.Users.Get*()` ?? l?y data
- [ ] Modify object theo logic
- [ ] G?i `.Users.Update()` ?? track changes
- [ ] G?i `await _unitOfWork.SaveChangesAsync()` ?? l?u
- [ ] N?u c?n transaction:
  - [ ] `BeginTransaction()` ? ??u
  - [ ] `CommitTransaction()` khi success
  - [ ] `RollbackTransaction()` trong catch block
- [ ] Write unit tests v?i Mock
- [ ] Write integration tests v?i real DB

---

## ?? Common Mistakes

### ? Mistake 1: Forget SaveChangesAsync

```csharp
// ? WRONG - Changes not saved!
user.FullName = "New Name";
_unitOfWork.Users.Update(user);
return true;  // Changes are NOT persisted

// ? CORRECT
user.FullName = "New Name";
_unitOfWork.Users.Update(user);
await _unitOfWork.SaveChangesAsync(cancellationToken);
return true;  // Changes ARE persisted
```

### ? Mistake 2: Not Checking Transaction Start

```csharp
// ? WRONG - No check
await _unitOfWork.BeginTransactionAsync();
// Do operations...

// ? CORRECT
var started = await _unitOfWork.BeginTransactionAsync();
if (!started)
    throw new Exception("Transaction failed");
// Do operations...
```

### ? Mistake 3: No Try-Catch with Transaction

```csharp
// ? WRONG - No rollback on error
await _unitOfWork.BeginTransactionAsync();
var user = await _unitOfWork.Users.GetByEmailAsync(email);
user.Balance -= amount;  // ? What if fails here?
await _unitOfWork.CommitTransactionAsync();

// ? CORRECT
try
{
    await _unitOfWork.BeginTransactionAsync();
    var user = await _unitOfWork.Users.GetByEmailAsync(email);
    user.Balance -= amount;
    await _unitOfWork.CommitTransactionAsync();
}
catch (Exception)
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

---

## ?? Resources

- [UNITOFWORK_QUICKREF.md](UNITOFWORK_QUICKREF.md) - Quick reference
- [UNITOFWORK_GUIDE.md](UNITOFWORK_GUIDE.md) - Detailed guide
- [JWT_AUTHENTICATION_SETUP.md](JWT_AUTHENTICATION_SETUP.md) - Auth setup

---

## ?? K?t Lu?n

V?i Unit of Work pattern:
? Code s?ch và d? hi?u
? D? test v?i mocks
? Transaction management ??n gi?n
? Consistent save logic
? Ready for production

**Happy Coding! ??**

