using PlusNine.Entities.Dtos.Requests;
using PlusNine.Entities.Dtos.Responses;

namespace PlusNine.Logic.Interfaces
{
    public interface IFriendService
    {
        Task<IEnumerable<UserSearchResponse>> SearchUsers(string username);
        Task<IEnumerable<FriendRequestResponse>> GetFriendRequests(Guid userId);
        Task<IEnumerable<FriendShipResponse>> GetFriends(Guid userId);
        Task SendFriendRequest(Guid userId, Guid recieverId);
        Task AcceptFriendRequest(Guid requestId);
        Task RejectFriendRequest(Guid requestId);
    }
}