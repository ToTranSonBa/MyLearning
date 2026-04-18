# Authentication Flow - Complete Guide

## Overview
This document describes the complete authentication system including registration, login, logout, password reset, and JWT token management.

## 1. Registration Flow

**Endpoint:** `POST /api/auth/register`

**Request:**
```json
{
  "userName": "john_doe",
  "email": "john@example.com",
  "password": "SecurePassword123!",
  "fullName": "John Doe"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64_encoded_refresh_token",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "userName": "john_doe",
  "email": "john@example.com"
}
```

**Flow:**
1. Validate email doesn't exist
2. Validate username doesn't exist
3. Hash password using BCrypt
4. Create new user record
5. Generate JWT access token (15 minutes expiry)
6. Generate refresh token (7 days expiry)
7. Store refresh token in database
8. Return tokens and user info

## 2. Login Flow

**Endpoint:** `POST /api/auth/login`

**Request:**
```json
{
  "emailOrUsername": "john_doe",
  "password": "SecurePassword123!"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "new_base64_encoded_refresh_token",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "userName": "john_doe",
  "email": "john@example.com"
}
```

**Flow:**
1. Find user by email or username
2. Verify password hash
3. Update last login timestamp
4. Generate new JWT access token
5. Generate new refresh token
6. Store refresh token in database
7. Return tokens and user info

## 3. Logout Flow

**Endpoint:** `POST /api/auth/logout`

**Headers:**
```
Authorization: Bearer <access_token>
```

**Response:**
```json
{
  "message": "Logged out successfully"
}
```

**Flow:**
1. Extract user ID from JWT claims
2. Find user by ID
3. Clear refresh token from database
4. Return success message

## 4. Refresh Token Flow

**Endpoint:** `POST /api/auth/refresh-token`

**Request:**
```json
{
  "refreshToken": "base64_encoded_refresh_token"
}
```

**Response:**
```json
{
  "accessToken": "new_eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "new_base64_encoded_refresh_token",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "userName": "john_doe",
  "email": "john@example.com"
}
```

**Flow:**
1. Find user by refresh token
2. Validate refresh token hasn't expired
3. Generate new access token
4. Generate new refresh token
5. Update refresh token in database
6. Return new tokens

## 5. Forgot Password Flow

**Endpoint:** `POST /api/auth/forgot-password`

**Request:**
```json
{
  "email": "john@example.com"
}
```

**Response:**
```json
{
  "message": "Password reset link has been sent to your email."
}
```

**Flow:**
1. Find user by email
2. Generate password reset token (unique, long-lived)
3. Set reset token expiry to 1 hour
4. Store reset token in database
5. Send reset link via email
6. Return success message

## 6. Reset Password Flow

**Endpoint:** `POST /api/auth/reset-password`

**Request:**
```json
{
  "token": "password_reset_token_from_email_link",
  "newPassword": "NewSecurePassword456!"
}
```

**Response:**
```json
{
  "message": "Password has been reset successfully."
}
```

**Flow:**
1. Find user by reset token
2. Validate reset token hasn't expired
3. Hash new password
4. Update user password
5. Clear reset token from database
6. Return success message

## JWT Token Structure

**Access Token:**
- Expires in: 15 minutes
- Contains:
  - `NameIdentifier` (UserId)
  - `Name` (UserName)
  - `Email`

**Refresh Token:**
- Expires in: 7 days
- Random 32-byte token converted to Base64
- Stored in database for validation

## Security Considerations

1. **Password Hashing:** Uses BCrypt with automatic salt
2. **Token Signing:** HS256 with 256+ bit secret key
3. **Token Validation:** 
   - Signature verification
   - Issuer validation
   - Audience validation
   - Lifetime validation
4. **Refresh Token Rotation:** New token generated on each refresh
5. **Token Expiry:** Short-lived access tokens with longer-lived refresh tokens

## Required NuGet Packages

- `System.IdentityModel.Tokens.Jwt` - JWT token handling
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT authentication
- `BCrypt.Net-Next` - Password hashing
- `MediatR` - CQRS pattern implementation

## Configuration

Update `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-that-is-256-bits-or-longer",
    "Issuer": "YourAppName",
    "Audience": "YourAppUsers"
  }
}
```

## Usage Example

```csharp
// Register
POST /api/auth/register
{
  "userName": "user1",
  "email": "user1@example.com",
  "password": "Pass@1234",
  "fullName": "User One"
}

// Login
POST /api/auth/login
{
  "emailOrUsername": "user1",
  "password": "Pass@1234"
}

// Use access token in subsequent requests
GET /api/users/profile
Authorization: Bearer <accessToken>

// Refresh token when expired
POST /api/auth/refresh-token
{
  "refreshToken": "<refreshToken>"
}

// Logout
POST /api/auth/logout
Authorization: Bearer <accessToken>

// Forgot password
POST /api/auth/forgot-password
{
  "email": "user1@example.com"
}

// Reset password (using link from email)
POST /api/auth/reset-password
{
  "token": "<resetTokenFromEmail>",
  "newPassword": "NewPass@5678"
}
```

## Database Migration

Apply migration to add authentication fields:

```bash
Add-Migration AddAuthenticationFields
Update-Database
```

## Protected Endpoints

To protect endpoints, add `[Authorize]` attribute:

```csharp
[HttpGet("profile")]
[Authorize]
public async Task<IActionResult> GetProfile()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    // ... implementation
}
```
