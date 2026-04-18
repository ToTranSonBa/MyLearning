using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.DTOs.AuthDto;
using Application.Features.Auth.Queries;
using MediatR;

namespace Application.Features.Auth.Commands;

/// <summary>
/// Login user with email/username and password.
/// Returns JWT tokens for API access.
/// Uses CQRS pattern with MediatR commands/queries.
/// </summary>
/// <example>
/// <code>
/// POST /api/auth/login
/// {
///   "emailOrUsername": "john_doe",
///   "password": "SecurePass123!"
/// }
/// 
/// Response (200 OK):
/// {
///   "accessToken": "eyJhbGciOiJIUzI1NiIs...",
///   "refreshToken": "K3Z8x9L2m5n7p0q3r6s9...",
///   "userId": "550e8400-e29b-41d4-a716-446655440000",
///   "userName": "john_doe",
///   "email": "john@example.com"
/// }
/// 
/// Error Response (401 Unauthorized):
/// {
///   "message": "Invalid email/username or password."
/// }
/// </code>
/// </example>
public record LoginCommand(string EmailOrUsername, string Password) : IRequest<AuthResponseDto>;

public class LoginHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public LoginHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    /// <summary>
    /// Handles user login with CQRS pattern:
    /// 1. Validate credentials
    /// 2. Generate access token
    /// 3. Generate refresh token
    /// 4. Store refresh token
    /// 5. Update last login timestamp
    /// </summary>
    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Command 1: Validate credentials
        var user = await _mediator.Send(
            new ValidateCredentialsCommand(request.EmailOrUsername, request.Password),
            cancellationToken);

        if (user is null)
            throw new UnauthorizedException("Invalid email/username or password.");

        // Command 2: Generate access token
        var accessToken = await _mediator.Send(
            new GenerateAccessTokenCommand(user),
            cancellationToken);

        // Command 3: Generate refresh token
        var refreshToken = await _mediator.Send(
            new GenerateRefreshTokenCommand(),
            cancellationToken);

        // Command 4: Store refresh token
        await _mediator.Send(
            new UpdateRefreshTokenCommand(user.Id, refreshToken, 7),
            cancellationToken);

        // Command 5: Update last login
        await _mediator.Send(
            new UpdateLastLoginCommand(user.Id),
            cancellationToken);

        return new AuthResponseDto(accessToken, refreshToken, user.Id, user.UserName, user.Email);
    }
}
