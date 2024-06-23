namespace PlusNine.Entities.Dtos.Responses
{
    public class FriendRequestResponse
    {
        public Guid Id { get; set; }
        public required string Username { get; set; }
        //public required Guid SenderId { get; set; }
    }
}