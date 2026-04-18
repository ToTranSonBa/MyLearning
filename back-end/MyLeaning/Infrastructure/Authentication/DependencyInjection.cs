using Infrastructure.Authentication.Services;
using Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Authentication;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        // Infrastructure-level services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<AuthService>();

        return services;
    }
}
