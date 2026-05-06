using Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Domain.Identity
{
    public class User : BaseEntity
    {
        public required Guid Id { get; set; }
        public required string UserName { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiryTime { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public new DateTime CreatedAt { get; set; }
        public new DateTime? UpdatedAt { get; set; }
    }
}
