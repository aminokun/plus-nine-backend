using System.ComponentModel.DataAnnotations;

namespace PlusNine.Entities.Dtos.Requests
{
    public class CreateUserRequest
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}
