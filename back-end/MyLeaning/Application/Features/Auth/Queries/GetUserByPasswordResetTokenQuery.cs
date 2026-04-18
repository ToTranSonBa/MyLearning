using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Identity;
using MediatR;

namespace Application.Features.Auth.Queries;

/// <summary>
/// Retrieves user by password reset token.
/// Returns null if token not found or expired.
/// </summary>
public record GetUserByPasswordResetTokenQuery(string ResetToken) : IRequest<User?>;

public class GetUserByPasswordResetTokenHandler : IRequestHandler<GetUserByPasswordResetTokenQuery, User?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByPasswordResetTokenHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Queries database for user with specified password reset token.
    /// Validates that token hasn't expired.
    /// </summary>
    public async Task<User?> Handle(GetUserByPasswordResetTokenQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ResetToken))
            throw new BadRequestException("Reset token cannot be empty.");

        var user = await _userRepository.GetByPasswordResetTokenAsync(request.ResetToken);

        if (user?.PasswordResetTokenExpiryTime < DateTime.UtcNow)
            return null;

        return user;
    }
}
