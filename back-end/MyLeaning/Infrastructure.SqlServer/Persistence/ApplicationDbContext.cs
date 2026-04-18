using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.SqlServer.Persistence
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ✔️ Apply tất cả configuration từ assembly
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // ✔️ Config Identity tables - loại bỏ tiền tố AspNet
            ConfigureIdentity(builder);
        }

        /// <summary>
        /// Configures Identity tables to remove AspNet prefix.
        /// Maps Identity tables to custom names:
        /// - AspNetUsers → Users
        /// - AspNetRoles → Roles
        /// - AspNetUserClaims → UserClaims
        /// - AspNetUserLogins → UserLogins
        /// - AspNetUserTokens → UserTokens
        /// - AspNetRoleClaims → RoleClaims
        /// - AspNetUserRoles → UserRoles
        /// </summary>
        private void ConfigureIdentity(ModelBuilder builder)
        {
            // Configure User table
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users", schema: "dbo");
                
                // Indexes
                entity.HasIndex(e => e.NormalizedEmail).HasDatabaseName("IDX_Users_NormalizedEmail");
                entity.HasIndex(e => e.NormalizedUserName).HasDatabaseName("IDX_Users_NormalizedUserName");
                entity.HasIndex(e => e.Email).HasDatabaseName("IDX_Users_Email");
            });

            // Configure Role table
            builder.Entity<IdentityRole<Guid>>(entity =>
            {
                entity.ToTable("Roles", schema: "dbo");
                
                // Indexes
                entity.HasIndex(e => e.NormalizedName).HasDatabaseName("IDX_Roles_NormalizedName");
            });

            // Configure UserClaim table
            builder.Entity<IdentityUserClaim<Guid>>(entity =>
            {
                entity.ToTable("UserClaims", schema: "dbo");
                
                // Indexes
                entity.HasIndex(e => e.UserId).HasDatabaseName("IDX_UserClaims_UserId");
            });

            // Configure UserLogin table
            builder.Entity<IdentityUserLogin<Guid>>(entity =>
            {
                entity.ToTable("UserLogins", schema: "dbo");
                
                // Composite key
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });
                
                // Indexes
                entity.HasIndex(e => e.UserId).HasDatabaseName("IDX_UserLogins_UserId");
            });

            // Configure UserToken table
            builder.Entity<IdentityUserToken<Guid>>(entity =>
            {
                entity.ToTable("UserTokens", schema: "dbo");
                
                // Composite key
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
            });

            // Configure RoleClaim table
            builder.Entity<IdentityRoleClaim<Guid>>(entity =>
            {
                entity.ToTable("RoleClaims", schema: "dbo");
                
                // Indexes
                entity.HasIndex(e => e.RoleId).HasDatabaseName("IDX_RoleClaims_RoleId");
            });

            // Configure UserRole table (junction)
            builder.Entity<IdentityUserRole<Guid>>(entity =>
            {
                entity.ToTable("UserRoles", schema: "dbo");
                
                // Composite key
                entity.HasKey(e => new { e.UserId, e.RoleId });
                
                // Indexes
                entity.HasIndex(e => e.RoleId).HasDatabaseName("IDX_UserRoles_RoleId");
            });
        }
    }
}