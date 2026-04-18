# CQRS Pattern Refactoring

## ?? Summary

?ã refactor t? **Service layer** ? **CQRS Query handlers** ? Application layer.

### ? Tr??c (Anti-pattern)
```
Infrastructure/Services/UserService.cs
?? GetUserByIdAsync()
?? GetUserByEmailAsync()
?? GetAllUsersAsync()

Web/Controllers/UsersController.cs
?? Inject IUserService
?? Call service methods directly
```

### ? Sau (CQRS Pattern)
```
Application/Features/Users/Queries/
?? GetUserByIdQuery.cs (Query + Handler)
?? GetUserByEmailQuery.cs (Query + Handler)
?? GetAllUsersQuery.cs (Query + Handler)

Application/Features/Users/Commands/
?? CreateUserCommand.cs
?? UpdateUserCommand.cs
?? DeleteUserCommand.cs

Web/Controllers/UsersController.cs
?? Inject IMediator
?? Send queries/commands
```

---

## ?? CQRS Nguyên T?c

**CQRS = Command Query Responsibility Segregation**

```
???????????????????????????
?    Client (Web)         ?
???????????????????????????
?  UsersController        ?
?  AuthController         ?
???????????????????????????
             ?
    ???????????????????
    ?                 ?
????????????????  ???????????????
? Commands     ?  ? Queries     ?
????????????????  ???????????????
? Write Data   ?  ? Read Data   ?
? Change State ?  ? No Changes  ?
?              ?  ?             ?
? Create       ?  ? GetById     ?
? Update       ?  ? GetByEmail  ?
? Delete       ?  ? GetAll      ?
? Login        ?  ?             ?
? Register     ?  ?             ?
????????????????  ???????????????
    ?                  ?
    ????????????????????
             ?
    ???????????????????
    ?   Handlers      ?
    ???????????????????
    ? Business Logic  ?
    ???????????????????
             ?
    ???????????????????
    ? Repositories    ?
    ???????????????????
             ?
    ???????????????????
    ?   Database      ?
    ???????????????????
```

---

## ?? Project Structure (Sau Refactoring)

```
Application/
??? Features/
?   ??? Auth/
?   ?   ??? Commands/
?   ?       ??? RegisterCommand.cs
?   ?       ??? LoginCommand.cs
?   ?       ??? LogoutCommand.cs
?   ?       ??? ForgotPasswordCommand.cs
?   ?       ??? ResetPasswordCommand.cs
?   ?       ??? RefreshTokenCommand.cs
?   ?
?   ??? Users/
?       ??? Commands/             ? Write operations
?       ?   ??? CreateUserCommand.cs
?       ?   ??? UpdateUserCommand.cs
?       ?   ??? DeleteUserCommand.cs
?       ?
?       ??? Queries/              ? Read operations
?           ??? GetUserByIdQuery.cs
?           ??? GetUserByEmailQuery.cs
?           ??? GetAllUsersQuery.cs
?
??? Common/
?   ??? Interfaces/
?       ??? IUnitOfWork.cs
?       ??? IUserRepository.cs
?       ??? ITokenService.cs
?       ??? IPasswordHasher.cs
?       ??? IEmailService.cs
?       ??? IAuthenticationService.cs

Infrastructure/
??? Authentication/
?   ??? Services/
?       ??? TokenService.cs
?       ??? PasswordHasher.cs
?       ??? EmailService.cs
?       ??? AuthenticationService.cs
?
??? SqlServer/
?   ??? Repositories/
?   ?   ??? UserRepository.cs
?   ??? UnitOfWork/
?       ??? UnitOfWork.cs
?
??? ? Services/ (Removed)
    ??? UserService.cs (Deleted!)

Web/
??? Controllers/
    ??? AuthController.cs
    ??? UsersController.cs
    ??? ...
```

---

## ?? Conversion: Service ? CQRS Query

### ? Tr??c (Service Method)
```csharp
// Infrastructure/Services/UserService.cs
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public async Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByGuidAsync(userId);
        if (user == null)
            return null;
        return new UserDto(user.Id, user.UserName, user.Email, user.FullName);
    }
}

// Web/Controllers/UsersController.cs
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;  // ? Service injection

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _userService.GetUserByIdAsync(id);  // ? Direct call
        return Ok(result);
    }
}
```

### ? Sau (CQRS Query Handler)
```csharp
// Application/Features/Users/Queries/GetUserByIdQuery.cs
public record GetUserByIdQuery(Guid Id) : IRequest<UserDto?>;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByGuidAsync(request.Id);
        if (user == null)
            return null;
        return new UserDto(user.Id, user.UserName, user.Email, user.FullName);
    }
}

// Web/Controllers/UsersController.cs
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;  // ? Mediator injection

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id));  // ? Send query
        return Ok(result);
    }
}
```

---

## ?? L?i Ích c?a CQRS

| L?i Ích | Gi?i Thích |
|---------|-----------|
| **Separation** | Commands (write) và Queries (read) riêng bi?t |
| **Scalability** | Có th? scale read và write riêng |
| **Testability** | D? test t?ng handler riêng |
| **Caching** | Có th? cache queries mà không lo side effects |
| **Audit Trail** | D? tracking commands (write operations) |
| **Performance** | Optimize queries khác v?i commands |
| **Maintainability** | Clear intent: query vs mutation |

---

## ?? Query Handlers M?i

### GetUserByIdQuery
```csharp
public record GetUserByIdQuery(Guid Id) : IRequest<UserDto?>;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByGuidAsync(request.Id);
        return user != null 
            ? new UserDto(user.Id, user.UserName, user.Email, user.FullName)
            : null;
    }
}
```

**Endpoint:**
```
GET /api/users/{id}
```

### GetUserByEmailQuery
```csharp
public record GetUserByEmailQuery(string Email) : IRequest<UserDto?>;

public class GetUserByEmailHandler : IRequestHandler<GetUserByEmailQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public async Task<UserDto?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        return user != null 
            ? new UserDto(user.Id, user.UserName, user.Email, user.FullName)
            : null;
    }
}
```

**Endpoint:**
```
GET /api/users/email/{email}
```

### GetAllUsersQuery
```csharp
public record GetAllUsersQuery : IRequest<IEnumerable<UserDto>>;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => new UserDto(u.Id, u.UserName, u.Email, u.FullName));
    }
}
```

**Endpoint:**
```
GET /api/users
```

---

## ?? Cách Thêm Query M?i

### Step 1: T?o Query Record
```csharp
public record GetUsersByRoleQuery(string Role) : IRequest<IEnumerable<UserDto>>;
```

### Step 2: T?o Handler
```csharp
public class GetUsersByRoleHandler : IRequestHandler<GetUsersByRoleQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public async Task<IEnumerable<UserDto>> Handle(GetUsersByRoleQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetByRoleAsync(request.Role);
        return users.Select(u => new UserDto(u.Id, u.UserName, u.Email, u.FullName));
    }
}
```

### Step 3: Add Repository Method (n?u c?n)
```csharp
public interface IUserRepository : IGenericRepository<Users>
{
    Task<IEnumerable<Users>> GetByRoleAsync(string role);
}
```

### Step 4: Implement Repository
```csharp
public class UserRepository : BaseRepository<Users>, IUserRepository
{
    public async Task<IEnumerable<Users>> GetByRoleAsync(string role)
    {
        return await _context.Set<Users>()
            .Where(u => u.Role == role)
            .ToListAsync();
    }
}
```

### Step 5: Use in Controller
```csharp
[HttpGet("role/{role}")]
public async Task<IActionResult> GetByRole(string role)
{
    var result = await _mediator.Send(new GetUsersByRoleQuery(role));
    return Ok(result);
}
```

---

## ?? Commands vs Queries

### Commands (Write Operations)
```csharp
// Create
public record CreateUserCommand(string UserName, string Email, string Password, string FullName) 
    : IRequest<bool>;

// Update
public record UpdateUserCommand(Guid UserId, string NewName) 
    : IRequest<bool>;

// Delete
public record DeleteUserCommand(Guid UserId) 
    : IRequest<bool>;
```

### Queries (Read Operations)
```csharp
// Get single
public record GetUserByIdQuery(Guid Id) 
    : IRequest<UserDto?>;

// Get by filter
public record GetUserByEmailQuery(string Email) 
    : IRequest<UserDto?>;

// Get list
public record GetAllUsersQuery 
    : IRequest<IEnumerable<UserDto>>;
```

---

## ?? Flow: Query ? Handler ? Result

```
1. Controller sends Query
   ?
UsersController.GetById(id)
   ?
2. MediatR routes to Handler
   ?
GetUserByIdHandler.Handle(query)
   ?
3. Handler uses Repository
   ?
_userRepository.GetByGuidAsync(id)
   ?
4. Repository queries Database
   ?
5. Returns UserDto
   ?
6. Controller returns response
   ?
HTTP 200 OK: { id, name, email, fullName }
```

---

## ? File Changes Summary

| Action | File |
|--------|------|
| ? Deleted | Infrastructure/Services/UserService.cs |
| ? Deleted | Application/Common/Interfaces/IUserService.cs |
| ? Created | Application/Features/Users/Queries/GetUserByIdQuery.cs |
| ? Created | Application/Features/Users/Queries/GetUserByEmailQuery.cs |
| ? Created | Application/Features/Users/Queries/GetAllUsersQuery.cs |
| ?? Updated | Web/Controllers/UsersController.cs |
| ?? Updated | Infrastructure/Authentication/DependencyInjection.cs |
| ?? Renamed | Querry ? Queries (folder name fix) |

---

## ?? Testing Queries

### Unit Test Example
```csharp
[TestClass]
public class GetUserByIdQueryTests
{
    [TestMethod]
    public async Task Handle_WithValidId_ReturnsUserDto()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();
        var userId = Guid.NewGuid();
        var user = new Users { Id = userId, UserName = "john", Email = "john@test.com", FullName = "John Doe" };
        
        mockRepo.Setup(r => r.GetByGuidAsync(userId))
            .ReturnsAsync(user);

        var handler = new GetUserByIdHandler(mockRepo.Object);
        var query = new GetUserByIdQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("john", result.UserName);
        mockRepo.Verify(r => r.GetByGuidAsync(userId), Times.Once);
    }
}
```

---

## ?? Next Steps

1. ? Create Command handlers for CreateUser, UpdateUser, DeleteUser
2. ? Add pagination to GetAllUsersQuery
3. ? Add filtering/searching to queries
4. ? Add caching for frequently accessed queries
5. ? Add validation to commands
6. ? Add logging/audit trails

---

## ?? Related Patterns

- **CQRS**: Command Query Responsibility Segregation
- **MediatR**: In-process messaging
- **Repository Pattern**: Data access abstraction
- **Handler Pattern**: Query/Command processing

---

## ? Summary

| Aspect | Before | After |
|--------|--------|-------|
| Query Location | Infrastructure/Services | Application/Features/Queries |
| Injection | IUserService | IMediator |
| Pattern | Service | CQRS |
| Testing | Service mock | Handler mock |
| Scaling | Single service | Independent handlers |
| Clarity | Generic service | Specific queries |

**Result: Clean CQRS architecture! ?**

