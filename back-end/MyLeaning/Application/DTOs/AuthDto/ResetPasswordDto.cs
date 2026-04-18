namespace Application.DTOs.AuthDto;

public record ResetPasswordDto(string Token, string NewPassword);
