namespace Application.DTOs.AuthDto;

/// <summary>
/// Login DTO for user authentication
/// </summary>
public record LoginDto(
    /// <summary>Email address or username</summary>
    string EmailOrUsername,
    /// <summary>User password</summary>
    string Password);
