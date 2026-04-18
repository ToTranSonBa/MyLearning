using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Identity;
using MediatR;

namespace Application.Features.Auth.Queries;

/// <summary>
/// Retrieves user by refresh token.
/// Returns null if token not found or expired.
/// </summary>
public record GetUserByRefreshTokenQuery(string RefreshToken) : IRequest<User?>;

public class GetUserByRefreshTokenHandler : IRequestHandler<GetUserByRefreshTokenQuery, User?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByRefreshTokenHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Queries database for user with specified refresh token.
    /// Validates that token hasn't expired.
    /// </summary>
    public async Task<User?> Handle(GetUserByRefreshTokenQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new BadRequestException("Refresh token cannot be empty.");

        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);

        if (user?.RefreshTokenExpiryTime < DateTime.UtcNow)
            return null;

        return user;
    }
}
