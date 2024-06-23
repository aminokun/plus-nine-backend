using System.ComponentModel.DataAnnotations;

namespace PlusNine.Logic.Models
{
    public class Pay
    {
        [Required]  
        public required string PriceId { get; set; }
    }
}
