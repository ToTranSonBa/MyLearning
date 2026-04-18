using Domain.Identity;
using Infrastructure.SqlServer.Persistence;

namespace Infrastructure.SqlServer.Mappings
{
    public static class UserMapping
    {
        //  Identity → Domain
        public static User ToDomain(this ApplicationUser user)
        {
            if (user == null) return null;

            return new User
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                CreatedAt = DateTime.UtcNow
            };
        }

        //  Domain → Identity
        public static ApplicationUser ToIdentity(this User user)
        {
            if (user == null) return null;

            return new ApplicationUser
            {
                Id = user.Id,
                UserName = user.UserName ?? user.Email,
                Email = user.Email,
                FullName = user.FullName,
                PasswordHash = user.PasswordHash,
                RefreshToken = user.RefreshToken,
                RefreshTokenExpiryTime = user.RefreshTokenExpiryTime ?? DateTime.UtcNow.AddMinutes(15) 
            };
        }
    }
}