using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands;

/// <summary>
/// Generates refresh token for token renewal.
/// Token expires in 7 days by default.
/// Used during registration and login.
/// </summary>
public record GenerateRefreshTokenCommand : IRequest<string>;

public class GenerateRefreshTokenHandler : IRequestHandler<GenerateRefreshTokenCommand, string>
{
    private readonly ITokenService _tokenService;

    public GenerateRefreshTokenHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    /// <summary>
    /// Generates a cryptographically secure refresh token.
    /// </summary>
    public async Task<string> Handle(GenerateRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(_tokenService.GenerateRefreshToken());
    }
}
