namespace PlusNine.Entities.Dtos.Responses
{
    public class GetUserResponse
    {
        public Guid UserId { get; set; }
        public required string Username { get; set; }
        public string? Token { get; set; }
        public DateTime TokenCreated { get; set; }
        public DateTime TokenExpires { get; set; }
    }
}
