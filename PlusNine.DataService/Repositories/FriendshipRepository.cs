using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlusNine.DataService.Data;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;


namespace PlusNine.DataService.Repositories
{
    public class FriendshipRepository : GenericRepository<Friendship>, IFriendshipRepository
    {
        public FriendshipRepository(AppDbContext context, ILogger logger) : base(context, logger)
        {
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
