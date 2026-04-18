using Application.Common.Interfaces;
using Domain.Identity;
using MediatR;

namespace Application.Features.Auth.Commands;

/// <summary>
/// Validates user credentials (email/username + password).
/// Returns user if credentials are valid, otherwise returns null.
/// Part of CQRS authentication refactoring.
/// </summary>
public record ValidateCredentialsCommand(string EmailOrUsername, string Password) : IRequest<User?>;

public class ValidateCredentialsHandler : IRequestHandler<ValidateCredentialsCommand, User?>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ValidateCredentialsHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Validates credentials by:
    /// 1. Finding user by email or username
    /// 2. Verifying password hash
    /// 3. Returning user if valid, null if invalid
    /// </summary>
    public async Task<User?> Handle(ValidateCredentialsCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.EmailOrUsername) ??
                   await _userRepository.GetByUsernameAsync(request.EmailOrUsername);

        if (user == null)
            return null;

        if (user.PasswordHash == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            return null;

        return user;
    }
}
