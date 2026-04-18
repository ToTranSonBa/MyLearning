using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Identity;
using MediatR;

namespace Application.Features.Auth.Commands;

/// <summary>
/// Hashes a password using secure BCrypt algorithm.
/// Returns the hashed password string.
/// </summary>
public record HashPasswordCommand(string Password) : IRequest<string>;

public class HashPasswordHandler : IRequestHandler<HashPasswordCommand, string>
{
    private readonly IPasswordHasher _passwordHasher;

    public HashPasswordHandler(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Hashes password using BCrypt with configured work factor.
    /// </summary>
    public async Task<string> Handle(HashPasswordCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
            throw new BadRequestException("Password cannot be empty.");

        return await Task.FromResult(_passwordHasher.HashPassword(request.Password));
    }
}
