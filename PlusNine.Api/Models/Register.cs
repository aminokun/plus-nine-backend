using System.ComponentModel.DataAnnotations;

namespace PlusNine.Api.Models
{
    public class Register
    {
        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string Password { get; set; }
        [Required]
        public required string ConfirmPassword { get; set; }
    }
}
