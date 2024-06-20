using PlusNine.Entities.DbSet;
using System.Linq.Expressions;


namespace PlusNine.DataService.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<IEnumerable<User>> GetAllAsync(Expression<Func<User, bool>> predicate);
        Task<User> GetByCustomerIdAsync(string customerId);
    }
}
