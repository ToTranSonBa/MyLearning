using Application.Common.Interfaces;
using Domain.Identity;
using MediatR;

namespace Application.Features.Auth.Commands;

/// <summary>
/// Generates JWT access token for authenticated user.
/// Token expires in 15 minutes by default.
/// </summary>
public record GenerateAccessTokenCommand(User User) : IRequest<string>;

public class GenerateAccessTokenHandler : IRequestHandler<GenerateAccessTokenCommand, string>
{
    private readonly ITokenService _tokenService;

    public GenerateAccessTokenHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    /// <summary>
    /// Generates JWT access token with user claims (Id, UserName, Email).
    /// </summary>
    public async Task<string> Handle(GenerateAccessTokenCommand request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(_tokenService.GenerateAccessToken(request.User));
    }
}
