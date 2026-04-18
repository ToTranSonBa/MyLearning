using Application.Features.Auth.Commands;
using Application.DTOs.AuthDto;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Controllers;

/// <summary>
/// Authentication endpoint handling user registration, login, and token management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    /// <param name="dto">Registration details (username, email, password, full name)</param>
    /// <returns>Authentication tokens and user information</returns>
    /// <response code="200">User registered successfully</response>
    /// <response code="400">Invalid input or user already exists</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        try
        {
            _logger.LogInformation("User registration attempt for email: {Email}", dto.Email);
            var command = new RegisterCommand(dto.UserName, dto.Email, dto.Password, dto.FullName);
            var result = await _mediator.Send(command);
            _logger.LogInformation("User registered successfully: {UserId}", result.UserId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Registration error: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Authenticate user with email/username and password.
    /// </summary>
    /// <param name="dto">Login credentials (email or username and password)</param>
    /// <returns>Authentication tokens and user information</returns>
    /// <response code="200">Login successful</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        try
        {
            _logger.LogInformation("Login attempt for: {EmailOrUsername}", dto.EmailOrUsername);
            var command = new LoginCommand(dto.EmailOrUsername, dto.Password);
            var result = await _mediator.Send(command);
            _logger.LogInformation("User logged in successfully: {UserId}", result.UserId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Login error: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Logout user by clearing authentication tokens.
    /// </summary>
    /// <returns>Success message</returns>
    /// <response code="200">Logged out successfully</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new LogoutCommand(Guid.Parse(userId));
        await _mediator.Send(command);
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Refresh access token using refresh token.
    /// </summary>
    /// <param name="dto">Refresh token</param>
    /// <returns>New authentication tokens</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="401">Invalid or expired refresh token</response>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        var command = new RefreshTokenCommand(dto.RefreshToken);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Request password reset via email.
    /// </summary>
    /// <param name="dto">Email address for password reset</param>
    /// <returns>Confirmation message</returns>
    /// <response code="200">Password reset email sent</response>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var command = new ForgotPasswordCommand(dto.Email);
        var result = await _mediator.Send(command);
        return Ok(new { message = result });
    }

    /// <summary>
    /// Reset password using token from email.
    /// </summary>
    /// <param name="dto">Reset token and new password</param>
    /// <returns>Confirmation message</returns>
    /// <response code="200">Password reset successfully</response>
    /// <response code="400">Invalid token or password</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var command = new ResetPasswordCommand(dto.Token, dto.NewPassword);
        var result = await _mediator.Send(command);
        return Ok(new { message = result });
    }
}
