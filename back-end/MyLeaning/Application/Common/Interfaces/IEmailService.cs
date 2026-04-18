namespace Application.Common.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string fullName, string resetLink);
    Task SendConfirmationEmailAsync(string email, string fullName, string confirmationLink);
}
