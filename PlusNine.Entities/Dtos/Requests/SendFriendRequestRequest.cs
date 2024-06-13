namespace PlusNine.Entities.Dtos.Requests
{
    public class SendFriendRequestRequest
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
    }
}