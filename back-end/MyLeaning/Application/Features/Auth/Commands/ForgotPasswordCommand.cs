using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Auth.Queries;
using MediatR;

namespace Application.Features.Auth.Commands;

/// <summary>
/// Initiates password reset process.
/// This command generates reset token and sends email via SendPasswordResetEmailCommand.
/// Uses CQRS pattern with MediatR commands/queries.
/// </summary>
public record ForgotPasswordCommand(string Email) : IRequest<string>;

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, string>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IMediator _mediator;

    public ForgotPasswordHandler(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        ITokenService tokenService,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _mediator = mediator;
    }

    /// <summary>
    /// Handles forgot password with CQRS pattern:
    /// 1. Find user by email
    /// 2. Generate reset token
    /// 3. Store token in database
    /// 4. Send email with reset link
    /// </summary>
    public async Task<string> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Query: Get user by email
        var user = await _mediator.Send(
            new GetUserByEmailQuery(request.Email),
            cancellationToken);

        if (user is null)
            throw new NotFoundException("User with this email not found.");

        // Generate reset token
        var resetToken = _tokenService.GeneratePasswordResetToken();
        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiryTime = DateTime.UtcNow.AddHours(1);

        // Save token
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send email via command (CQRS style)
        var resetLink = $"https://yourdomain.com/reset-password?token={resetToken}";
        await _mediator.Send(
            new SendPasswordResetEmailCommand(user.Email, user.FullName, resetLink),
            cancellationToken);

        return "Password reset link has been sent to your email.";
    }
}
