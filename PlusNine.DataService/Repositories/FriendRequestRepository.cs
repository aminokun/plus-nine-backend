using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlusNine.DataService.Data;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlusNine.DataService.Repositories
{
    public class FriendRequestRepository : GenericRepository<FriendRequest>, IFriendRequestRepository
    {
        public FriendRequestRepository(AppDbContext context, ILogger logger) : base(context, logger)
        {
        }
        public override async Task<bool> Update(FriendRequest friendRequest)
        {
            try
            {
                var result = await _dbSet.FirstOrDefaultAsync(x => x.Id == friendRequest.Id);

                if (result == null)
                {
                    return false;
                }

                result.UpdatedDate = DateTime.Now;
                result.FriendShipStatus = friendRequest.FriendShipStatus;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} Update Function Error", typeof(FriendRequestRepository));
                throw;
            }
        }
    }
}
