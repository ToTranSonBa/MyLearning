using Domain.Identity;

namespace Application.Common.Interfaces;

/// <summary>
/// Repository interface for User aggregate.
/// Defines contracts for user data access operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets user by email.
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets user by username.
    /// </summary>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Gets user by GUID ID.
    /// </summary>
    Task<User?> GetByGuidAsync(Guid id);

    /// <summary>
    /// Gets user by refresh token.
    /// </summary>
    Task<User?> GetByRefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Gets user by password reset token.
    /// </summary>
    Task<User?> GetByPasswordResetTokenAsync(string resetToken);

    /// <summary>
    /// Checks if email is unique.
    /// </summary>
    Task<bool> IsEmailUniqueAsync(string email);

    /// <summary>
    /// Checks if username is unique.
    /// </summary>
    Task<bool> IsUsernameUniqueAsync(string username);

    /// <summary>
    /// Gets all users.
    /// </summary>
    Task<IEnumerable<User>> GetAllAsync();

    /// <summary>
    /// Adds a new user.
    /// </summary>
    Task AddAsync(User entity);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    Task Update(User entity);

    /// <summary>
    /// Deletes a user.
    /// </summary>
    void Delete(User entity);
}