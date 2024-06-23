using PlusNine.Entities.DbSet;
using System.Linq.Expressions;


namespace PlusNine.DataService.Repositories.Interfaces
{
    public interface IFriendshipRepository : IGenericRepository<Friendship>
    {
        Task<IEnumerable<Friendship>> GetAllAsync(Expression<Func<Friendship, bool>> predicate);
    }
}
