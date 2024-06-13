namespace PlusNine.Logic.Interfaces
{
    public interface IFriendHub
    {
        Task SendFriendRequestNotification(Guid receiverId);
        Task NotifyFriendRequestAccepted(Guid senderId);
        Task NotifyFriendRequestRejected(Guid senderId);
    }
}
