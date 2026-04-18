# JWT Authentication Setup Guide

## Overview
Complete authentication system with JWT tokens, refresh tokens, password reset, and email verification.

## Implementation Summary

### 1. Core Components Created

#### Authentication Services
- **TokenService**: Generates JWT access tokens, refresh tokens, and password reset tokens
- **PasswordHasher**: Secures passwords using BCrypt hashing
- **EmailService**: Handles password reset and confirmation emails

#### MediatR Commands
- **RegisterCommand**: User registration with validation
- **LoginCommand**: User authentication
- **LogoutCommand**: Token invalidation
- **RefreshTokenCommand**: Generate new access token
- **ForgotPasswordCommand**: Initiate password reset
- **ResetPasswordCommand**: Complete password reset

#### DTOs (Data Transfer Objects)
- `AuthResponseDto`: Contains access token, refresh token, user info
- `LoginDto`: Email/username and password
- `RegisterDto`: User registration data
- `RefreshTokenDto`: Refresh token request
- `ForgotPasswordDto`: Email for password reset
- `ResetPasswordDto`: Reset token and new password

#### API Controller
- `AuthController`: Exposes all authentication endpoints

### 2. Database Schema Updates

The `Users` entity has been updated with the following fields:
```csharp
public string? PasswordHash { get; set; }
public string? RefreshToken { get; set; }
public DateTime? RefreshTokenExpiryTime { get; set; }
public string? PasswordResetToken { get; set; }
public DateTime? PasswordResetTokenExpiryTime { get; set; }
public DateTime? LastLoginAt { get; set; }
public bool IsEmailConfirmed { get; set; }
public DateTime CreatedAt { get; set; }
public DateTime? UpdatedAt { get; set; }
```

### 3. Configuration

Add to `appsettings.json`:
```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-that-is-at-least-256-bits-or-longer",
    "Issuer": "YourAppName",
    "Audience": "YourAppUsers"
  }
}
```

### 4. Required NuGet Packages

Infrastructure project:
- `System.IdentityModel.Tokens.Jwt` v8.0.0
- `Microsoft.IdentityModel.Tokens` v8.14.0
- `BCrypt.Net-Next` v4.0.3
- `Microsoft.Extensions.Configuration` v10.0.5
- `Microsoft.Extensions.Logging` v10.0.5

Web project:
- `MediatR` v14.1.0
- `Microsoft.AspNetCore.Authentication.JwtBearer` v10.0.6

## API Endpoints

### 1. Register
```
POST /api/auth/register
Content-Type: application/json

{
  "userName": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass@123",
  "fullName": "John Doe"
}

Response:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "base64_encoded_token",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "userName": "john_doe",
  "email": "john@example.com"
}
```

### 2. Login
```
POST /api/auth/login
Content-Type: application/json

{
  "emailOrUsername": "john_doe",
  "password": "SecurePass@123"
}

Response: Same as register
```

### 3. Refresh Token
```
POST /api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "base64_encoded_token"
}

Response: New access and refresh tokens
```

### 4. Logout
```
POST /api/auth/logout
Authorization: Bearer <accessToken>

Response:
{
  "message": "Logged out successfully"
}
```

### 5. Forgot Password
```
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "john@example.com"
}

Response:
{
  "message": "Password reset link has been sent to your email."
}
```

### 6. Reset Password
```
POST /api/auth/reset-password
Content-Type: application/json

{
  "token": "reset_token_from_email",
  "newPassword": "NewSecurePass@456"
}

Response:
{
  "message": "Password has been reset successfully."
}
```

## Protecting Endpoints

Add `[Authorize]` attribute to require authentication:

```csharp
[HttpGet("profile")]
[Authorize]
public async Task<IActionResult> GetProfile()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    // Your code
}
```

## Token Lifetimes

- **Access Token**: 15 minutes
- **Refresh Token**: 7 days
- **Password Reset Token**: 1 hour

## Security Features

1. **Password Hashing**: BCrypt with automatic salt generation
2. **JWT Signing**: HS256 algorithm with 256+ bit secret
3. **Token Validation**: 
   - Signature verification
   - Issuer validation
   - Audience validation
   - Lifetime validation
4. **Refresh Token Rotation**: New token on each refresh
5. **Token Revocation**: Logout clears refresh token

## Testing with Postman

1. **Register a user** - Copy the `accessToken`
2. **Set Authorization header** - Type: Bearer Token, paste the token
3. **Call protected endpoints** - They will work with the token
4. **Use refresh token** - Call refresh-token endpoint to get new access token
5. **Logout** - Call logout to invalidate the refresh token

## Next Steps

1. Implement email sending in `EmailService` (currently logs to console)
2. Add rate limiting for authentication endpoints
3. Add CORS configuration if frontend is on different domain
4. Add email verification flow
5. Add two-factor authentication (optional)
6. Add role-based authorization

## Troubleshooting

### "The service collection cannot be modified because it is read-only"
- Ensure all service registrations happen **before** `builder.Build()`

### Invalid JWT token
- Check `Jwt:SecretKey` is at least 256 bits (32 bytes)
- Verify issuer and audience match between token generation and validation

### Refresh token expired
- Check `RefreshTokenExpiryTime` in database
- Token expires 7 days after creation by default

## Project Structure

```
Authentication Flow:
??? Infrastructure/
?   ??? Authentication/
?       ??? DependencyInjection.cs
?       ??? Services/
?           ??? TokenService.cs
?           ??? PasswordHasher.cs
?           ??? EmailService.cs
??? Application/
?   ??? Common/Interfaces/
?   ?   ??? ITokenService.cs
?   ?   ??? IPasswordHasher.cs
?   ?   ??? IEmailService.cs
?   ??? DTOs/AuthDto/
?   ?   ??? AuthResponseDto.cs
?   ?   ??? LoginDto.cs
?   ?   ??? RegisterDto.cs
?   ?   ??? ...
?   ??? Features/Auth/Commands/
?       ??? RegisterCommand.cs
?       ??? LoginCommand.cs
?       ??? LogoutCommand.cs
?       ??? RefreshTokenCommand.cs
?       ??? ForgotPasswordCommand.cs
?       ??? ResetPasswordCommand.cs
??? Web/
    ??? Controllers/
    ?   ??? AuthController.cs
    ??? Program.cs (JWT middleware configured)
```

