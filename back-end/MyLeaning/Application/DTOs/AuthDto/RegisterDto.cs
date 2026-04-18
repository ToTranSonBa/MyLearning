namespace Application.DTOs.AuthDto;

/// <summary>
/// Register DTO for new user registration
/// </summary>
public record RegisterDto(
    /// <summary>Unique username (3-50 characters, alphanumeric, underscore, hyphen)</summary>
    string UserName,
    /// <summary>Email address (must be valid email format)</summary>
    string Email,
    /// <summary>Password (min 8 chars, 1 uppercase, 1 lowercase, 1 digit, 1 special char)</summary>
    string Password,
    /// <summary>Full name of the user (2-100 characters)</summary>
    string FullName);
