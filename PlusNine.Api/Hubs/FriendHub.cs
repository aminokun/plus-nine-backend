using Microsoft.AspNetCore.SignalR;
using PlusNine.Logic.Interfaces;

namespace PlusNine.Api.Hubs
{
    public class FriendHub : Hub, IFriendHub
    {
        public async Task SendFriendRequestNotification()
        {
            await Clients.Others.SendAsync("ReceiveNotification");
        }

        public async Task NotifyFriendRequestAccepted(Guid senderId)
        {
            await Clients.User(senderId.ToString()).SendAsync("FriendRequestAccepted");
        }

        public async Task NotifyFriendRequestRejected(Guid senderId)
        {
            await Clients.User(senderId.ToString()).SendAsync("FriendRequestRejected");
        }
    }
}