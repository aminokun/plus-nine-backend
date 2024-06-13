using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlusNine.DataService.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IObjectiveRepository Objectives { get; }
        IUserRepository User { get; }
        IFriendRequestRepository FriendRequest { get; }
        IFriendshipRepository Friendship { get; }
        Task<bool> CompleteAsync();
    }
}
