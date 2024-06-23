using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlusNine.DataService.Data;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;
using System.Linq.Expressions;


namespace PlusNine.DataService.Repositories
{
    public class FriendshipRepository : GenericRepository<Friendship>, IFriendshipRepository
    {
        public FriendshipRepository(AppDbContext context, ILogger logger) : base(context, logger)
        {
        }
        public async Task<IEnumerable<Friendship>> GetAllAsync(Expression<Func<Friendship, bool>> predicate)
        {
            try
            {
                return await _dbSet.Where(predicate)
                    .Where(x => x.Status == 1)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .OrderBy(x => x.AddedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} GetAllAsync Function Error", typeof(FriendshipRepository));
                throw;
            }
        }
        public override async Task<bool> Update(Friendship friendship)
        {
            try
            {
                var result = await _dbSet.FirstOrDefaultAsync(x => x.Id == friendship.Id);

                if (result == null)
                {
                    return false;
                }

                result.UpdatedDate = DateTime.Now;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} Update Function Error", typeof(FriendshipRepository));
                throw;
            }
        }
    }
}
