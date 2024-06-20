using System.ComponentModel.DataAnnotations;

namespace PlusNine.Entities.DbSet
{
    public class User : BaseEntity
    {
        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string Email { get; set; }
        [Required]
        public string CustomerId { get; set; }
        [Required]
        public required string Role { get; set; } = "Member";

        [Required]
        public byte[] PasswordSalt { get; set; }
        [Required]
        public byte[] PasswordHash { get; set; }

        public string? Token { get; set; }
        public DateTime TokenCreated { get; set; }
        public DateTime TokenExpires { get; set; }
    }
}
