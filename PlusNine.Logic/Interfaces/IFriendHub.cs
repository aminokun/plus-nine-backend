namespace PlusNine.Logic.Interfaces
{
    public interface IFriendHub
    {
        Task SendFriendRequestNotification();
        Task NotifyFriendRequestAccepted(Guid senderId);
        Task NotifyFriendRequestRejected(Guid senderId);
    }
}
