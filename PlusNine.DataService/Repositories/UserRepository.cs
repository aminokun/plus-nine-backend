using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlusNine.DataService.Data;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlusNine.DataService.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context, ILogger logger) : base(context, logger)
        {
        }

        public async Task<User> SingleOrDefaultAsync(Func<User, bool> predicate)
        {
            try
            {
                return await Task.FromResult(_dbSet.AsNoTracking().SingleOrDefault(predicate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} SingleOrDefaultAsync Function Error", typeof(UserRepository));
                throw;
            }
        }

        public override async Task<IEnumerable<User>> All()
        {
            try
            {
                return await _dbSet.Where(x => x.Status == 1)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .OrderBy(x => x.AddedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} All Function Error", typeof(UserRepository));
                throw;
            }
        }

        public override async Task<bool> Delete(Guid id)
        {
            try
            {
                var result = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);

                if (result == null)
                {
                    return false;
                }

                result.Status = 0;
                result.UpdatedDate = DateTime.Now;

                _dbSet.Update(result);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} Delete Function Error", typeof(UserRepository));
                throw;
            }
        }

        public override async Task<bool> Update(User user)
        {
            try
            {
                var existingUser = await _dbSet.FirstOrDefaultAsync(x => x.Id == user.Id);

                if (existingUser == null)
                {
                    return false;
                }

                existingUser.UserName = user.UserName;
                existingUser.Token = user.Token;
                existingUser.TokenCreated = user.TokenCreated;
                existingUser.TokenExpires = user.TokenExpires;
                existingUser.UpdatedDate = DateTime.Now;

                _dbSet.Update(existingUser);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} Update Function Error", typeof(UserRepository));
                throw;
            }
        }
    }
}
