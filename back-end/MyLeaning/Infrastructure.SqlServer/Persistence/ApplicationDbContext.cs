using Domain.FlashCard;
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
            ConfigureFlashCard(builder);
        }

        private void ConfigureFlashCard(ModelBuilder builder)
        {
            builder.Entity<Deck>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("Deck", schema: "dbo");
                
                // Indexes
                entity.HasIndex(e => e.AuthorId).HasDatabaseName("IDX_Deck_AuthorId");
            });
            builder.Entity<Card>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("Card", schema: "dbo");
                    
                // Indexes
                entity.HasIndex(e => e.DeckId).HasDatabaseName("IDX_Card_DeckId");
            });
    
            builder.Entity<UserDeck>(entity =>
            {
                entity.HasKey(entity => new { entity.UserId, entity.DeckId });
                entity.ToTable("UserDeck", schema: "dbo");
                    
                // Composite key
                entity.HasKey(e => new { e.UserId, e.DeckId });
                    
                // Indexes
                entity.HasIndex(e => e.UserId).HasDatabaseName("IDX_UserDeck_UserId");
                entity.HasIndex(e => e.DeckId).HasDatabaseName("IDX_UserDeck_DeckId");
            });
            builder.Entity<CardMetaData>(entity =>
            {
                entity.HasKey(e => new { e.CardId, e.Key });
                entity.ToTable("CardMetaData", schema: "dbo");
                    
                // Indexes
                entity.HasIndex(e => e.CardId).HasDatabaseName("IDX_CardMetaData_CardId");
            });
            builder.Entity<FlashcardReview>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.CardId });
                entity.ToTable("UserCardProcess", schema: "dbo");
                    
                // Composite key
                entity.HasKey(e => new { e.UserId, e.CardId });
                    
                // Indexes
                entity.HasIndex(e => e.UserId).HasDatabaseName("IDX_UserCardProcess_UserId");
                entity.HasIndex(e => e.CardId).HasDatabaseName("IDX_UserCardProcess_CardId");
            });
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