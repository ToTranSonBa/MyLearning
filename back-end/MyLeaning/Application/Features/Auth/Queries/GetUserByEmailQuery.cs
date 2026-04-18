using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Identity;
using MediatR;

namespace Application.Features.Auth.Queries;

/// <summary>
/// Retrieves user by email address.
/// Returns null if user not found.
/// </summary>
public record GetUserByEmailQuery(string Email) : IRequest<User?>;

public class GetUserByEmailHandler : IRequestHandler<GetUserByEmailQuery, User?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByEmailHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Queries database for user with specified email.
    /// </summary>
    public async Task<User?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new BadRequestException("Email cannot be empty.");

        return await _userRepository.GetByEmailAsync(request.Email);
    }
}
