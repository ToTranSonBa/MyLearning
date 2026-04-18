# Clean Architecture: IUnitOfWork Design

## ?? Nguyên T?c

**IUnitOfWork ch? qu?n lý: SAVE & TRANSACTION**
**IRepository ch? qu?n lý: QUERY**

```
? SAI - IUnitOfWork ch?a repositories
public interface IUnitOfWork
{
    IUserRepository Users { get; }  // ? Wrong!
    Task SaveChangesAsync();
}

? ?ÚNG - IUnitOfWork ch? cho save/transaction
public interface IUnitOfWork
{
    Task SaveChangesAsync();
    Task<bool> BeginTransactionAsync();
    Task<bool> CommitTransactionAsync();
    Task<bool> RollbackTransactionAsync();
}
```

---

## ?? Architecture Diagram

```
???????????????????????????????????????????????????????
?           Application Layer                          ?
???????????????????????????????????????????????????????
?  Defines Contracts (Business Logic)                 ?
?  ?? IAuthenticationService                          ?
?  ?? IUserService                                    ?
?  ?? IUnitOfWork (Persistence contract)              ?
?  ?? IUserRepository (Query contract)                ?
???????????????????????????????????????????????????????
             ?                          ?
             ? Depends on               ? Depends on
             ? (loose coupling)         ? (loose coupling)
             ?                          ?
?????????????????????????    ??????????????????????????
? Infrastructure.Auth   ?    ? Infrastructure.SqlServer?
?????????????????????????    ?????????????????????????
? AuthenticationService ?    ? UnitOfWork            ?
? - Uses IUnitOfWork    ?    ? - Manages DbContext   ?
? - Uses IUserRepository?    ? - Handles transactions?
?                       ?    ? - SaveChanges         ?
?????????????????????????    ?????????????????????????
                                       ?
                             ?????????????????????????
                             ? UserRepository        ?
                             ?????????????????????????
                             ? - Queries users       ?
                             ? - Adds/Updates users  ?
                             ? - Uses DbContext      ?
                             ?????????????????????????
                                       ?
                             ?????????????????????????
                             ? DbContext             ?
                             ?????????????????????????
                             ? Entity Framework Core ?
                             ? Change Tracking       ?
                             ?????????????????????????
                                       ?
                             ?????????????????????????
                             ? SQL Server Database   ?
                             ?????????????????????????
```

---

## ?? Separation of Concerns

### IUserRepository
**Responsibility:** QUERY operations
```csharp
public interface IUserRepository : IGenericRepository<Users>
{
    Task<Users?> GetByEmailAsync(string email);
    Task<Users?> GetByUsernameAsync(string username);
    Task<Users?> GetByGuidAsync(Guid id);
    Task<IEnumerable<Users>> GetAllAsync();
    // Query methods ONLY - no save/transaction
}
```

### IUnitOfWork
**Responsibility:** PERSISTENCE & TRANSACTION coordination
```csharp
public interface IUnitOfWork : IAsyncDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<bool> CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task<bool> RollbackTransactionAsync(CancellationToken cancellationToken = default);
    // Save & transaction methods ONLY - no repository references
}
```

---

## ?? Usage Pattern

### Simple Operation (No Transaction)
```csharp
public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;              // ? For SAVE
    private readonly IUserRepository _userRepository;       // ? For QUERY

    public async Task<bool> RegisterAsync(...)
    {
        // 1. QUERY: Check if email exists
        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing != null)
            throw new InvalidOperationException("Email already registered");

        // 2. CREATE: Prepare new user (no DB access yet)
        var user = new Users { Email = email, ... };

        // 3. ADD: Track in DbContext (via repository)
        await _userRepository.AddAsync(user);

        // 4. SAVE: Persist to database (via IUnitOfWork)
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
```

### Complex Operation (With Transaction)
```csharp
public async Task<bool> TransferAsync(Guid senderId, Guid receiverId, decimal amount)
{
    // 1. BEGIN: Start transaction
    var started = await _unitOfWork.BeginTransactionAsync();
    if (!started)
        throw new Exception("Transaction failed to start");

    try
    {
        // 2. QUERY: Get both users
        var sender = await _userRepository.GetByGuidAsync(senderId);
        var receiver = await _userRepository.GetByGuidAsync(receiverId);

        // 3. MODIFY: Update balances
        sender.Balance -= amount;
        receiver.Balance += amount;

        // 4. TRACK: Update in DbContext
        _userRepository.Update(sender);
        _userRepository.Update(receiver);

        // 5. COMMIT: Save all changes as atomic unit
        await _unitOfWork.CommitTransactionAsync();
        return true;
    }
    catch (Exception)
    {
        // 6. ROLLBACK: Discard all changes on error
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

---

## ?? Dependency Injection

### Register Services
```csharp
// Infrastructure.SqlServer/DependencyInjection.cs
services.AddDbContext<ApplicationDbContext>(...);

// Register IUnitOfWork (for save & transaction)
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register IUserRepository (for queries)
services.AddScoped<IUserRepository, UserRepository>();
```

### Register Application Services
```csharp
// Infrastructure/Authentication/DependencyInjection.cs
services.AddScoped<IAuthenticationService, AuthenticationService>();
services.AddScoped<IUserService, UserService>();

// AuthenticationService gets both:
// - IUnitOfWork (injected)
// - IUserRepository (injected)
```

### Usage in Handler
```csharp
public class LoginHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IAuthenticationService _authService;

    public LoginHandler(IAuthenticationService authService)
    {
        _authService = authService;  // All dependencies hidden inside
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(
            request.EmailOrUsername,
            request.Password,
            cancellationToken);
    }
}
```

---

## ? Benefits of This Design

| Benefit | Description |
|---------|-------------|
| **Single Responsibility** | IUnitOfWork only handles save/transactions |
| **Separation of Concerns** | Query (IRepository) vs Persistence (IUnitOfWork) |
| **Testability** | Mock IUnitOfWork and IUserRepository separately |
| **Flexibility** | Easy to swap persistence strategy |
| **Clarity** | Clear what each interface does |
| **Maintainability** | Easy to understand and modify |

---

## ?? Common Mistakes to Avoid

### ? Mistake 1: IUnitOfWork Contains Repositories
```csharp
// WRONG
public interface IUnitOfWork
{
    IUserRepository Users { get; }      // ? Don't do this!
    IProductRepository Products { get; } // ? Don't do this!
    Task SaveChangesAsync();
}
```

**Fix:** Inject repositories separately
```csharp
// CORRECT
public interface IUnitOfWork
{
    Task SaveChangesAsync();
    Task<bool> BeginTransactionAsync();
    Task<bool> CommitTransactionAsync();
    Task<bool> RollbackTransactionAsync();
}

// And inject IUserRepository separately
var user = await _userRepository.GetByEmailAsync(email);
```

### ? Mistake 2: Using IUnitOfWork for Queries
```csharp
// WRONG
var user = await _unitOfWork.Users.GetByEmailAsync(email);

// CORRECT
var user = await _userRepository.GetByEmailAsync(email);
```

### ? Mistake 3: Forgetting to Save
```csharp
// WRONG
user.FullName = "New Name";
_userRepository.Update(user);
// Forgot to save!

// CORRECT
user.FullName = "New Name";
_userRepository.Update(user);
await _unitOfWork.SaveChangesAsync();  // ? Don't forget!
```

### ? Mistake 4: No Try-Catch with Transaction
```csharp
// WRONG
await _unitOfWork.BeginTransactionAsync();
var user = await _userRepository.GetByGuidAsync(userId);
user.Balance -= 100;  // What if this fails?
await _unitOfWork.CommitTransactionAsync();

// CORRECT
try
{
    await _unitOfWork.BeginTransactionAsync();
    var user = await _userRepository.GetByGuidAsync(userId);
    user.Balance -= 100;
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

---

## ?? Comparison: Before vs After

### ? Before (Wrong)
```
IUnitOfWork
?? IUserRepository Users { get; }
?? IProductRepository Products { get; }
?? SaveChangesAsync()
?? BeginTransactionAsync()
?? CommitTransactionAsync()

Problems:
- Multiple responsibilities
- Hard to understand
- Violates Single Responsibility Principle
- Tightly coupled
```

### ? After (Correct)
```
IUnitOfWork
?? SaveChangesAsync()
?? BeginTransactionAsync()
?? CommitTransactionAsync()
?? RollbackTransactionAsync()

IUserRepository
?? GetByEmailAsync()
?? GetByUsernameAsync()
?? AddAsync()
?? Update()
?? Delete()

Benefits:
- Single responsibility each
- Clear purpose
- Easy to understand
- Loosely coupled
```

---

## ?? Key Takeaway

```
Think of IUnitOfWork like a CASHIER:
- Takes your payment (SaveChangesAsync)
- Starts a transaction (BeginTransactionAsync)
- Commits the sale (CommitTransactionAsync)
- Refunds if needed (RollbackTransactionAsync)

Think of IRepository like a SALES CLERK:
- Finds products you want (GetByEmailAsync)
- Adds items to cart (AddAsync)
- Updates quantities (Update)
```

**They work together but have different jobs!**

---

## ?? Related Files

- `Application/Common/Interfaces/IUnitOfWork.cs` - UnitOfWork contract
- `Application/Common/Interfaces/IUserRepository.cs` - Repository contract
- `Infrastructure.SqlServer/UnitOfWork/UnitOfWork.cs` - UnitOfWork implementation
- `Infrastructure.SqlServer/Repositories/UserRepository.cs` - Repository implementation
- `Infrastructure/Services/AuthenticationService.cs` - Example usage

---

## ? Summary

| Aspect | IUnitOfWork | IUserRepository |
|--------|------------|-----------------|
| **Purpose** | Save & Transaction | Query & Modify |
| **Methods** | SaveChanges, Begin, Commit, Rollback | Get, Add, Update, Delete |
| **Contains** | Nothing else | Query logic only |
| **Used For** | Persisting changes | Accessing data |
| **Dependency** | DbContext | DbContext |

**Result: Clean, maintainable, testable code! ?**

