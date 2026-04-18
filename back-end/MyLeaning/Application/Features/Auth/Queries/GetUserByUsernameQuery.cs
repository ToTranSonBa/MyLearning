using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Identity;
using MediatR;

namespace Application.Features.Auth.Queries;

/// <summary>
/// Retrieves user by username.
/// Returns null if user not found.
/// </summary>
public record GetUserByUsernameQuery(string Username) : IRequest<User?>;

public class GetUserByUsernameHandler : IRequestHandler<GetUserByUsernameQuery, User?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByUsernameHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Queries database for user with specified username.
    /// </summary>
    public async Task<User?> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            throw new BadRequestException("Username cannot be empty.");

        return await _userRepository.GetByUsernameAsync(request.Username);
    }
}
