using Application.Common.Exceptions;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Auth.Queries;

/// <summary>
/// Checks if email is unique (not already registered).
/// Returns true if email is available, false if already in use.
/// </summary>
public record IsEmailUniqueQuery(string Email) : IRequest<bool>;

public class IsEmailUniqueHandler : IRequestHandler<IsEmailUniqueQuery, bool>
{
    private readonly IUserRepository _userRepository;

    public IsEmailUniqueHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Checks database for existing user with specified email.
    /// </summary>
    public async Task<bool> Handle(IsEmailUniqueQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BadRequestException("Email cannot be empty.");

        return await _userRepository.IsEmailUniqueAsync(request.Email);
    }
}
