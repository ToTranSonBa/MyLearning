using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Auth.Queries;
using MediatR;

namespace Application.Features.Auth.Commands;

/// <summary>
/// Resets user password using valid reset token.
/// Uses CQRS pattern with MediatR commands/queries.
/// </summary>
public record ResetPasswordCommand(string Token, string NewPassword) : IRequest<string>;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, string>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IMediator _mediator;

    public ResetPasswordHandler(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _mediator = mediator;
    }

    /// <summary>
    /// Handles password reset with CQRS pattern:
    /// 1. Validate reset token
    /// 2. Hash new password
    /// 3. Update user password
    /// 4. Clear reset token
    /// </summary>
    public async Task<string> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Query: Get user by reset token
        var user = await _mediator.Send(
            new GetUserByPasswordResetTokenQuery(request.Token),
            cancellationToken);

        if (user is null)
            throw new UnauthorizedException("Invalid or expired reset token.");

        // Command: Hash new password
        var newPasswordHash = await _mediator.Send(
            new HashPasswordCommand(request.NewPassword),
            cancellationToken);

        user.PasswordHash = newPasswordHash;
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiryTime = null;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return "Password has been reset successfully.";
    }
}
