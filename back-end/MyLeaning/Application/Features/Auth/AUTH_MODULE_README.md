# AUTH Module - Vertical Slice Architecture

## Overview

The AUTH module implements secure user authentication with registration, login, and token management following Clean Architecture and CQRS patterns.

## Features

- ? User registration with email and username validation
- ? Secure password hashing using BCrypt
- ? JWT-based authentication
- ? Access and refresh token generation
- ? Login by email or username
- ? Password reset functionality
- ? User session tracking (last login)
- ? Email confirmation flow
- ? Comprehensive input validation with FluentValidation

## Architecture

### Layer Structure

```
Domain Layer
??? Users (Entity)

Application Layer
??? Features/Auth
?   ??? Commands
?   ?   ??? RegisterCommand + RegisterHandler
?   ?   ??? LoginCommand + LoginHandler
?   ?   ??? LogoutCommand + LogoutHandler
?   ?   ??? RefreshTokenCommand + RefreshTokenHandler
?   ?   ??? Password management commands
?   ??? Validators
?   ?   ??? RegisterValidator
?   ?   ??? LoginValidator
?   ??? Behaviors
?   ?   ??? ValidationBehavior (MediatR pipeline)
?   ??? Tests
?       ??? AuthCommandHandlersTests
?       ??? AuthIntegrationTests
??? DTOs/AuthDto
    ??? RegisterDto
    ??? LoginDto
    ??? AuthResponseDto
    ??? Token DTOs

Infrastructure Layer
??? Authentication/Services
?   ??? PasswordHasher (BCrypt)
?   ??? TokenService (JWT)
?   ??? EmailService
?   ??? AuthService (Coordinator)
??? Authentication/DependencyInjection

Infrastructure.SqlServer Layer
??? Repositories
?   ??? UserRepository (CRUD + Auth queries)
??? Persistence
?   ??? ApplicationDbContext
??? UnitOfWork

Presentation Layer
??? Controllers
    ??? AuthController (REST endpoints)
```

## API Endpoints

### Register User
```http
POST /api/auth/register
Content-Type: application/json

{
  "userName": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass123!",
  "fullName": "John Doe"
}

Response (200 OK):
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "K3Z8x9L2m5n7p0q3r6s9...",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "userName": "john_doe",
  "email": "john@example.com"
}

Error (400 Bad Request):
{
  "message": "Email already registered."
}
```

### Login User
```http
POST /api/auth/login
Content-Type: application/json

{
  "emailOrUsername": "john_doe",
  "password": "SecurePass123!"
}

Response (200 OK):
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "K3Z8x9L2m5n7p0q3r6s9...",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "userName": "john_doe",
  "email": "john@example.com"
}

Error (401 Unauthorized):
{
  "message": "Invalid email/username or password."
}
```

### Logout User
```http
POST /api/auth/logout
Authorization: Bearer <access_token>

Response (200 OK):
{
  "message": "Logged out successfully"
}
```

### Refresh Token
```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "K3Z8x9L2m5n7p0q3r6s9..."
}

Response (200 OK):
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "new_refresh_token...",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "userName": "john_doe",
  "email": "john@example.com"
}
```

## Validation Rules

### Username
- Required
- 3-50 characters
- Alphanumeric, underscore, hyphen only
- Must be unique

### Email
- Required
- Valid email format (RFC 5322)
- Max 100 characters
- Must be unique

### Password
- Required
- Minimum 8 characters
- Maximum 128 characters
- Must contain uppercase letter
- Must contain lowercase letter
- Must contain digit
- Must contain special character (!@#$%^&*()...)

### Full Name
- Required
- 2-100 characters

## Security Implementation

### Password Hashing
- **Algorithm**: BCrypt with salt
- **Work factor**: 10 (configurable)
- **Implementation**: Infrastructure.Authentication.Services.PasswordHasher
- **Verification**: Secure comparison preventing timing attacks

### JWT Tokens
- **Algorithm**: HMAC-SHA256
- **Access Token Expiry**: 15 minutes
- **Refresh Token Expiry**: 7 days
- **Claims**: UserId, UserName, Email
- **Issued By**: Configuration-driven issuer
- **Audience**: Configuration-driven audience

### Input Validation
- All inputs validated via FluentValidation
- MediatR pipeline behavior enforces validation
- Returns 400 Bad Request with validation errors
- Prevents malicious input early in request pipeline

## Database Schema

### Users Table
```sql
CREATE TABLE [dbo].[Users] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY,
    [UserName] NVARCHAR(50) UNIQUE NOT NULL,
    [FullName] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(100) UNIQUE NOT NULL,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [RefreshToken] NVARCHAR(MAX),
    [RefreshTokenExpiryTime] DATETIME2,
    [PasswordResetToken] NVARCHAR(MAX),
    [PasswordResetTokenExpiryTime] DATETIME2,
    [LastLoginAt] DATETIME2,
    [IsEmailConfirmed] BIT DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL,
    [UpdatedAt] DATETIME2
);

CREATE INDEX IDX_Users_Email ON [dbo].[Users]([Email]);
CREATE INDEX IDX_Users_UserName ON [dbo].[Users]([UserName]);
```

## Configuration

### appsettings.json
```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-min-256-bits-for-sha256",
    "Issuer": "YourAppName",
    "Audience": "YourAppUsers",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "FromEmail": "noreply@yourapp.com",
    "FromName": "Your App"
  }
}
```

## Usage Examples

### Register New User (C#)
```csharp
var command = new RegisterCommand(
    "john_doe",
    "john@example.com",
    "SecurePass123!",
    "John Doe"
);

var result = await mediator.Send(command);
// Returns AuthResponseDto with tokens
```

### Login User (C#)
```csharp
var command = new LoginCommand("john@example.com", "SecurePass123!");
var result = await mediator.Send(command);
// Returns AuthResponseDto with tokens
```

### Using Access Token
```csharp
// Add to HTTP header
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...

// Claims automatically populated
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var userName = User.FindFirst(ClaimTypes.Name)?.Value;
var email = User.FindFirst(ClaimTypes.Email)?.Value;
```

## Testing

### Unit Tests
- Test command handlers in isolation
- Mock dependencies (repositories, services)
- Verify business logic and validation
- Location: `Application\Features\Auth\Tests\AuthCommandHandlersTests.cs`

### Integration Tests
- Test end-to-end authentication flows
- Register -> Login cycle
- Token generation and validation
- Location: `Application\Features\Auth\Tests\AuthIntegrationTests.cs`

### Running Tests
```bash
dotnet test Application.UnitTests
dotnet test Application.IntegrationTests
```

## Error Handling

### Validation Errors (400)
```json
{
  "message": "Username must be at least 3 characters.; Email format is invalid."
}
```

### Duplicate User (400)
```json
{
  "message": "Email already registered."
}
```

### Invalid Credentials (401)
```json
{
  "message": "Invalid email/username or password."
}
```

### Unauthorized (401)
```json
{
  "message": "Unauthorized access."
}
```

## Performance Considerations

- **Password Hashing**: Async by default, runs on thread pool
- **Token Generation**: Minimal overhead (~1-2ms)
- **User Lookup**: Indexed by email and username for O(1) lookups
- **Caching**: Refresh tokens cached in Redis (optional)
- **Concurrency**: Thread-safe implementations

## Security Best Practices

? Passwords hashed with BCrypt (never stored in plain text)
? JWT tokens signed with secret key
? Refresh tokens stored securely and rotated
? Input validation prevents injection attacks
? HTTPS enforced in production
? CORS properly configured
? Rate limiting recommended on endpoints
? Audit logging for sensitive operations

## Future Enhancements

- [ ] Multi-factor authentication (MFA)
- [ ] Social OAuth integration (Google, GitHub)
- [ ] API key authentication for service-to-service
- [ ] Permission-based authorization
- [ ] Audit logging and monitoring
- [ ] Account lockout after failed attempts
- [ ] CAPTCHA for registration
- [ ] Email verification before login

## Dependencies

- **BCrypt.Net-Next**: Password hashing
- **System.IdentityModel.Tokens.Jwt**: JWT token generation
- **FluentValidation**: Input validation
- **MediatR**: CQRS command handling
- **Entity Framework Core**: Data access

## License

Same as main project
