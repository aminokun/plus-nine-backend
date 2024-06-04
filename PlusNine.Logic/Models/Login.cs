using System.ComponentModel.DataAnnotations;

namespace PlusNine.Logic.Models
{
    public class Login
    {
        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string Password { get; set; }
    }
}
