using PlusNine.Entities.Dtos.Requests;
using PlusNine.Entities.Dtos.Responses;

namespace PlusNine.Logic.Interfaces
{
    public interface IFriendService
    {
        Task<IEnumerable<UserSearchResponse>> SearchUsers(string username);
        Task SendFriendRequest(SendFriendRequestRequest request);
        Task AcceptFriendRequest(Guid requestId);
        Task RejectFriendRequest(Guid requestId);
    }
}