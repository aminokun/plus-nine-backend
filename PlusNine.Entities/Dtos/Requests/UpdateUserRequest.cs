using System.ComponentModel.DataAnnotations;

namespace PlusNine.Entities.Dtos.Requests
{
    public class UpdateUserRequest
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string CustomerId { get; set; }
        [Required]
        public required string Role { get; set; }
        [Required]
        public required byte[] PasswordSalt { get; set; }
        [Required]
        public required byte[] PasswordHash { get; set; }

        public string? Token { get; set; }
        public DateTime TokenCreated { get; set; }
        public DateTime TokenExpires { get; set; }
    }
}
