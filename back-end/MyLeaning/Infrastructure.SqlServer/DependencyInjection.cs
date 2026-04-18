using Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Infrastructure.SqlServer.Persistence;
using Infrastructure.SqlServer.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Infrastructure.SqlServer;

public static class DependencyInjection
{
    public static IServiceCollection AddSqlServerInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServerConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString,
                // Chỉ định nơi lưu trữ Migration là chính project này
                builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Register IUnitOfWork (Transaction & Persistence coordinator)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        return services;
    }

}