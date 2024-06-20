using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PlusNine.Logic.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace PlusNine.Logic.Hubs
{
    [Authorize]
    public class FriendHub : Hub
    {
        private readonly IHubContext<FriendHub> _hubContext;

        public FriendHub(IHubContext<FriendHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public override async Task OnConnectedAsync()
        {
            var accessToken = Context.GetHttpContext().Request.Cookies["X-Access-Token"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(accessToken);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                }
            }
            await base.OnConnectedAsync();
        }
        public async Task SendFriendRequestNotification()
        {
            await Clients.All.SendAsync("ReceiveNotification");
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