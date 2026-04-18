using Application.Common.Interfaces;
using Domain.Identity;
using Infrastructure.SqlServer.Mappings;
using Infrastructure.SqlServer.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Infrastructure.SqlServer.Repositories;

/// <summary>
/// Repository implementation for User aggregate.
/// Provides database access for user operations.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UserRepository(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    /// <summary>
    /// Gets user by email.
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return (await _userManager.FindByEmailAsync(email))?.ToDomain();
    }

    /// <summary>
    /// Gets user by username.
    /// </summary>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return (await _userManager.FindByNameAsync(username))?.ToDomain();
    }

    /// <summary>
    /// Gets user by GUID ID.
    /// </summary>
    public async Task<User?> GetByGuidAsync(Guid id)
    {
        return (await _userManager.FindByIdAsync(id.ToString()))?.ToDomain();
    }


    /// <summary>
    /// Gets user by refresh token.
    /// </summary>
    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return (await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken))?.ToDomain();
    }

    /// <summary>
    /// Gets user by password reset token.
    /// </summary>
    public async Task<User?> GetByPasswordResetTokenAsync(string resetToken)
    {
        throw new NotImplementedException("GetByPasswordResetTokenAsync is not implemented yet.");
    }

    /// <summary>
    /// Checks if email is unique.
    /// </summary>
    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        var user = await GetByEmailAsync(email);
        return user == null;
    }

    /// <summary>
    /// Checks if username is unique.
    /// </summary>
    public async Task<bool> IsUsernameUniqueAsync(string username)
    {
        var user = await GetByUsernameAsync(username);
        return user == null;
    }

    /// <summary>
    /// Gets all users.
    /// </summary>
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return (await _context.Users.ToListAsync()).Select(a => a.ToDomain());
    }

    /// <summary>
    /// Adds a new user.
    /// </summary>
    public async Task AddAsync(User entity)
    {
        await _userManager.CreateAsync(entity.ToIdentity());
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    public async Task Update(User entity)
    {
        var appUser = await _userManager.FindByNameAsync(entity.UserName);
        if (appUser is null) return;

        appUser.FullName = entity.FullName;
        appUser.Email = entity.Email;
        appUser.RefreshToken = entity.RefreshToken;
        appUser.RefreshTokenExpiryTime = entity.RefreshTokenExpiryTime;

        await _userManager.UpdateAsync(appUser);
    }

    /// <summary>
    /// Deletes a user.
    /// </summary>
    public void Delete(User entity)
    {
        _context.Remove(entity.ToIdentity());
    }

}