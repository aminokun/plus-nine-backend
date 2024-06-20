using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Responses;
using PlusNine.Logic.Interfaces;
using PlusNine.Logic.Hubs;

namespace PlusNine.Logic
{
    public class FriendService : IFriendService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<FriendHub> _hubContext;


        public FriendService(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<FriendHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<IEnumerable<FriendRequestResponse>> GetFriendRequests(Guid userId)
        {
            var friendRequests = await _unitOfWork.FriendRequest.GetAllAsync(fr => fr.ReceiverId == userId && fr.FriendShipStatus == FriendRequestStatus.Pending);
            var friendRequestResponses = new List<FriendRequestResponse>();

            foreach (var friendRequest in friendRequests)
            {
                var sender = await _unitOfWork.User.GetByIdAsync(friendRequest.SenderId);
                var friendRequestResponse = new FriendRequestResponse
                {
                    Id = friendRequest.Id,
                    SenderId = sender.Id
                };
                friendRequestResponses.Add(friendRequestResponse);
            }

            return friendRequestResponses;
        }

        public async Task SendFriendRequest(Guid userId, Guid receiverId)
        {
            var friendRequest = _mapper.Map<FriendRequest>((userId, receiverId)); 
            await _unitOfWork.FriendRequest.Add(friendRequest);
            await _unitOfWork.CompleteAsync();

            await _hubContext.Clients.Group(receiverId.ToString()).SendAsync("ReceiveFriendRequest");
            //await _hubContext.Clients.All.SendAsync("ReceiveFriendRequest", "message");

        }

        public async Task AcceptFriendRequest(Guid requestId)
        {
            var friendRequest = await _unitOfWork.FriendRequest.GetByIdAsync(requestId);

            if (friendRequest == null || friendRequest.FriendShipStatus != FriendRequestStatus.Pending)
            {
                throw new ArgumentException("Invalid friend request.");
            }

            friendRequest.FriendShipStatus = FriendRequestStatus.Accepted;
            friendRequest.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.FriendRequest.Update(friendRequest);

            var friendship = _mapper.Map<Friendship>(friendRequest);
            await _unitOfWork.Friendship.Add(friendship);
            await _unitOfWork.CompleteAsync();

            await _hubContext.Clients.Group(friendRequest.SenderId.ToString()).SendAsync("FriendRequestAccepted");
        }

        public async Task RejectFriendRequest(Guid requestId)
        {
            var friendRequest = await _unitOfWork.FriendRequest.GetByIdAsync(requestId);

            if (friendRequest == null || friendRequest.FriendShipStatus != FriendRequestStatus.Pending)
            {
                throw new ArgumentException("Invalid friend request.");
            }

            friendRequest.FriendShipStatus = FriendRequestStatus.Rejected;
            friendRequest.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.FriendRequest.Update(friendRequest);
            await _unitOfWork.CompleteAsync();

            await _hubContext.Clients.Group(friendRequest.SenderId.ToString()).SendAsync("FriendRequestRejected");

        }

        public async Task<IEnumerable<UserSearchResponse>> SearchUsers(string username)
        {
            var users = await _unitOfWork.User.GetAllAsync(u => u.UserName.Contains(username));
            var userResponses = _mapper.Map<IEnumerable<UserSearchResponse>>(users);
            return userResponses;
        }
    }
}