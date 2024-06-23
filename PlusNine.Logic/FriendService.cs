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
                    Username = sender.UserName,
                };
                friendRequestResponses.Add(friendRequestResponse);
            }

            return friendRequestResponses;
        }

        public async Task<IEnumerable<FriendShipResponse>> GetFriends(Guid userId)
        {
            var friendShips = await _unitOfWork.Friendship.GetAllAsync(fs => fs.User1Id == userId || fs.User2Id == userId);
            var friendShipResponses = new List<FriendShipResponse>();

            foreach (var friendShip in friendShips)
            {
                Guid friendId = userId != friendShip.User1Id ? friendShip.User1Id : friendShip.User2Id;

                var friend = await _unitOfWork.User.GetByIdAsync(friendId);

                var friendShipResponse = new FriendShipResponse
                {
                    FriendId = friend.Id,
                    Username = friend.UserName
                };

                friendShipResponses.Add(friendShipResponse);
            }

            return friendShipResponses;
        }



        public async Task SendFriendRequest(Guid userId, Guid receiverId)
        {

            // Check if user is trying to send a friend request to themselvess
            if (userId == receiverId)
            {
                throw new ArgumentException("You cannot send a friend request to yourself.");
            }

            // Check if already a pending friend request from userId to receiverId
            var existingSentRequest = await _unitOfWork.FriendRequest.SingleOrDefaultAsync(fr =>
                fr.SenderId == userId && fr.ReceiverId == receiverId && fr.FriendShipStatus == FriendRequestStatus.Pending);

            if (existingSentRequest != null)
            {
                throw new ArgumentException("Wait for the current friend request to be accepted or denied.");
            }

            // Check if already a pending friend request frm receiver to user
            var existingReceivedRequest = await _unitOfWork.FriendRequest.SingleOrDefaultAsync(fr =>
                fr.SenderId == receiverId && fr.ReceiverId == userId && fr.FriendShipStatus == FriendRequestStatus.Pending);

            if (existingReceivedRequest != null)
            {
                throw new ArgumentException("The receiver has already sent you a friend request. Accept or reject it first.");
            }

            // Check iff already friends
            var existingFriendship = await _unitOfWork.Friendship.SingleOrDefaultAsync(f =>
                (f.User1Id == userId && f.User2Id == receiverId) ||
                (f.User1Id == receiverId && f.User2Id == userId));

            if (existingFriendship != null)
            {
                throw new ArgumentException("You are already friends with this user.");
            }

            // Create and send the friend request
            var friendRequest = new FriendRequest
            {
                SenderId = userId,
                ReceiverId = receiverId,
                FriendShipStatus = FriendRequestStatus.Pending,
                Status = 1
            };

            await _unitOfWork.FriendRequest.Add(friendRequest);
            await _unitOfWork.CompleteAsync();

            await _hubContext.Clients.Group(receiverId.ToString()).SendAsync("ReceiveFriendRequest");
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