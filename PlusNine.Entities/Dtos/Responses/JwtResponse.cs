namespace PlusNine.Entities.Dtos.Responses
{
    public class JwtResponse
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string CustomerId { get; set; }
        public string Role { get; set; }
    }
}
