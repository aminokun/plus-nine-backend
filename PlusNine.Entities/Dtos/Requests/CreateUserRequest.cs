using System.ComponentModel.DataAnnotations;

namespace PlusNine.Entities.Dtos.Requests
{
    public class CreateUserRequest
    {
        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string Email { get; set; }

        [Required]
        [MinLength(6)]
        public required string Password { get; set; }

        [Required]
        [Compare("Password")]
        public required string ConfirmPassword { get; set; }
    }
}
