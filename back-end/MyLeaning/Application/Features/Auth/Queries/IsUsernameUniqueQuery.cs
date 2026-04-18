using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Identity;
using MediatR;

namespace Application.Features.Auth.Queries;

/// <summary>
/// Checks if username is unique (not already registered).
/// Returns true if username is available, false if already in use.
/// </summary>
public record IsUsernameUniqueQuery(string Username) : IRequest<bool>;

public class IsUsernameUniqueHandler : IRequestHandler<IsUsernameUniqueQuery, bool>
{
    private readonly IUserRepository _userRepository;

    public IsUsernameUniqueHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Checks database for existing user with specified username.
    /// </summary>
    public async Task<bool> Handle(IsUsernameUniqueQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new BadRequestException("Username cannot be empty.");

        return await _userRepository.IsUsernameUniqueAsync(request.Username);
    }
}
