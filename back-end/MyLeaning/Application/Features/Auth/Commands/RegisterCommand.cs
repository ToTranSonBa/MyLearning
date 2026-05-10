using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Mapping;
using Application.DTOs.AuthDto;
using Application.Features.Auth.Queries;
using Domain.Identity;
using MediatR;

namespace Application.Features.Auth.Commands;

/// <summary>
/// Register new user with authentication.
/// Sends confirmation email via SendConfirmationEmailCommand.
/// Uses CQRS pattern with MediatR commands/queries.
/// </summary>
/// <example>
/// <code>
/// POST /api/auth/register
/// {
///   "userName": "john_doe",
///   "email": "john@example.com",
///   "password": "SecurePass123!",
///   "fullName": "John Doe"
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
/// </code>
/// </example>
public record RegisterCommand(RegisterDto register) : IRequest<AuthResponseDto>;

public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IMediator _mediator;

    public RegisterHandler(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _mediator = mediator;
    }

    /// <summary>
    /// Handles user registration with CQRS pattern:
    /// 1. Validate email uniqueness
    /// 2. Validate username uniqueness
    /// 3. Hash password
    /// 4. Create user
    /// 5. Generate tokens
    /// 6. Send confirmation email
    /// </summary>
    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Query 1: Check email uniqueness
        var isEmailUnique = await _mediator.Send(
            new IsEmailUniqueQuery(request.register.Email),
            cancellationToken);
        if (!isEmailUnique)
            throw new BadRequestException("Email already registered.");

        // Query 2: Check username uniqueness
        var isUsernameUnique = await _mediator.Send(
            new IsUsernameUniqueQuery(request.register.UserName),
            cancellationToken);
        if (!isUsernameUnique)
            throw new BadRequestException("Username already taken.");

        // Command 1: Hash password
        var passwordHash = await _mediator.Send(
            new HashPasswordCommand(request.register.Password),
            cancellationToken);

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = request.register.UserName,
            Email = request.register.Email,
            FullName = request.register.FullName,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            IsEmailConfirmed = false
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

        // Send confirmation email via command (CQRS style)
        var confirmationLink = $"https://yourdomain.com/confirm-email?token={user.Id}";
        await _mediator.Send(
            new SendConfirmationEmailCommand(user.Email, user.FullName, confirmationLink),
            cancellationToken);

        return new AuthResponseDto(accessToken, refreshToken, user.ToUserDto());
    }
}
