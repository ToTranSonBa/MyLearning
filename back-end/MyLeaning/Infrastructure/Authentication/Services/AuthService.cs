using Application.Common.Interfaces;
using Domain.Identity;
using MediatR;

namespace Infrastructure.Authentication.Services;

/// <summary>
/// Coordinates authentication operations using CQRS pattern.
/// Acts as a facade for CQRS commands and queries.
/// This is optional - you can also inject IMediator directly in handlers.
/// </summary>
public class AuthService
{
    private readonly IMediator _mediator;

    public AuthService(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Validates user credentials and returns authentication status.
    /// Delegates to ValidateCredentialsCommand.
    /// </summary>
    public async Task<(bool IsValid, User? User)> ValidateCredentialsAsync(string emailOrUsername, string password)
    {
        var user = await _mediator.Send(new Application.Features.Auth.Commands.ValidateCredentialsCommand(emailOrUsername, password));
        return (user is not null, user);
    }

    /// <summary>
    /// Validates email uniqueness for registration.
    /// Delegates to IsEmailUniqueQuery.
    /// </summary>
    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        return await _mediator.Send(new Application.Features.Auth.Queries.IsEmailUniqueQuery(email));
    }

    /// <summary>
    /// Validates username uniqueness for registration.
    /// Delegates to IsUsernameUniqueQuery.
    /// </summary>
    public async Task<bool> IsUsernameUniqueAsync(string username)
    {
        return await _mediator.Send(new Application.Features.Auth.Queries.IsUsernameUniqueQuery(username));
    }

    /// <summary>
    /// Hashes a password using BCrypt algorithm.
    /// Delegates to HashPasswordCommand.
    /// </summary>
    public async Task<string> HashPasswordAsync(string password)
    {
        return await _mediator.Send(new Application.Features.Auth.Commands.HashPasswordCommand(password));
    }

    /// <summary>
    /// Generates JWT access token for user.
    /// Delegates to GenerateAccessTokenCommand.
    /// </summary>
    public async Task<string> GenerateAccessTokenAsync(User user)
    {
        return await _mediator.Send(new Application.Features.Auth.Commands.GenerateAccessTokenCommand(user));
    }

    /// <summary>
    /// Generates refresh token for token renewal.
    /// Delegates to GenerateRefreshTokenCommand.
    /// </summary>
    public async Task<string> GenerateRefreshTokenAsync()
    {
        return await _mediator.Send(new Application.Features.Auth.Commands.GenerateRefreshTokenCommand());
    }
}
