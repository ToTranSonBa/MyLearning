using Domain.Identity;

namespace Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string GeneratePasswordResetToken();
}
