using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.DTOs.AuthDto;
using Application.Features.Auth.Queries;
using MediatR;

namespace Application.Features.Auth.Commands;

/// <summary>
/// Refreshes expired access token using valid refresh token.
/// Returns new access token and refresh token.
/// Uses CQRS pattern with MediatR commands/queries.
/// </summary>
public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponseDto>;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IMediator _mediator;

    public RefreshTokenHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Handles token refresh with CQRS pattern:
    /// 1. Validate refresh token
    /// 2. Generate new access token
    /// 3. Generate new refresh token
    /// 4. Store new refresh token
    /// </summary>
    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Query: Get user by refresh token
        var user = await _mediator.Send(
            new GetUserByRefreshTokenQuery(request.RefreshToken),
            cancellationToken);

        if (user is null)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        // Command 1: Generate new access token
        var newAccessToken = await _mediator.Send(
            new GenerateAccessTokenCommand(user),
            cancellationToken);

        // Command 2: Generate new refresh token
        var newRefreshToken = await _mediator.Send(
            new GenerateRefreshTokenCommand(),
            cancellationToken);

        // Command 3: Store new refresh token
        await _mediator.Send(
            new UpdateRefreshTokenCommand(user.Id, newRefreshToken, 7),
            cancellationToken);

        return new AuthResponseDto(newAccessToken, newRefreshToken, user.Id, user.UserName, user.Email);
    }
}
