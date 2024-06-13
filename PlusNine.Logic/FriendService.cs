using AutoMapper;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Requests;
using PlusNine.Entities.Dtos.Responses;
using PlusNine.Logic.Interfaces;

namespace PlusNine.Logic
{
    public class FriendService : IFriendService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFriendHub _friendHub;

        public FriendService(IUnitOfWork unitOfWork, IMapper mapper, IFriendHub friendHub)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _friendHub = friendHub;
        }

        public async Task SendFriendRequest(SendFriendRequestRequest request)
        {
            var friendRequest = _mapper.Map<FriendRequest>(request);
            await _unitOfWork.FriendRequest.Add(friendRequest);
            await _unitOfWork.CompleteAsync();

            await _friendHub.SendFriendRequestNotification(request.ReceiverId);
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

            await _friendHub.NotifyFriendRequestAccepted(friendRequest.SenderId);
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

            await _friendHub.NotifyFriendRequestRejected(friendRequest.SenderId);
        }

        public async Task<IEnumerable<UserSearchResponse>> SearchUsers(string username)
        {
            var users = await _unitOfWork.User.GetAllAsync(u => u.UserName.Contains(username));
            var userResponses = _mapper.Map<IEnumerable<UserSearchResponse>>(users);
            return userResponses;
        }
    }
}