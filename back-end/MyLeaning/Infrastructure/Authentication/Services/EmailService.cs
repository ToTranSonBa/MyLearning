using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Authentication.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string email, string fullName, string resetLink)
    {
        try
        {
            var subject = "Password Reset Request";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>Hi {fullName},</p>
                <p>You requested to reset your password. Click the link below to proceed:</p>
                <a href='{resetLink}'>Reset Password</a>
                <p>This link will expire in 1 hour.</p>
                <p>If you didn't request this, please ignore this email.</p>
            ";

            // TODO: Implement actual email sending logic
            _logger.LogInformation($"Password reset email sent to {email}");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending password reset email: {ex.Message}");
            throw;
        }
    }

    public async Task SendConfirmationEmailAsync(string email, string fullName, string confirmationLink)
    {
        try
        {
            var subject = "Email Confirmation";
            var body = $@"
                <h2>Confirm Your Email</h2>
                <p>Hi {fullName},</p>
                <p>Click the link below to confirm your email:</p>
                <a href='{confirmationLink}'>Confirm Email</a>
                <p>If you didn't create this account, please ignore this email.</p>
            ";

            // TODO: Implement actual email sending logic
            _logger.LogInformation($"Confirmation email sent to {email}");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending confirmation email: {ex.Message}");
            throw;
        }
    }
}
