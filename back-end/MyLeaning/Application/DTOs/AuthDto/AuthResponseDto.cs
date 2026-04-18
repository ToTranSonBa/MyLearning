namespace Application.DTOs.AuthDto;

/// <summary>
/// Authentication response containing JWT tokens and user information
/// </summary>
public record AuthResponseDto(
    /// <summary>JWT access token (valid for 15 minutes)</summary>
    string AccessToken,
    /// <summary>Refresh token for token renewal (valid for 7 days)</summary>
    string RefreshToken,
    /// <summary>User ID</summary>
    Guid UserId,
    /// <summary>Username</summary>
    string UserName,
    /// <summary>User email address</summary>
    string Email);
