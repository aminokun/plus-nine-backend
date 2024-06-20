using PlusNine.Entities.DbSet;
using System.Linq.Expressions;


namespace PlusNine.DataService.Repositories.Interfaces
{
    public interface IFriendRequestRepository : IGenericRepository<FriendRequest>
    {
        Task<IEnumerable<FriendRequest>> GetAllAsync(Expression<Func<FriendRequest, bool>> predicate);
    }
}
