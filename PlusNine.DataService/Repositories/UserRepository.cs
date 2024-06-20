using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlusNine.DataService.Data;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;
using System.Linq.Expressions;


namespace PlusNine.DataService.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context, ILogger logger) : base(context, logger)
        {
        }
        public override async Task<User> GetByIdAsync(Guid userId)
        {
            try
            {
                var user = await _dbSet.Where(x => x.Id == userId)
                    .Where(x => x.Status == 1)
                    .Select(x => new User
                    {
                        Id = x.Id,
                        UserName = x.UserName,
                        Role = x.Role,
                        Email = x.Email,
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", userId);
                    return null;
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} GetByIdAsync Function Error", typeof(UserRepository));
                throw;
            }
        }

        public async Task<User> GetByCustomerIdAsync(string customerId)
        {
            try
            {
                var user = await _dbSet.Where(x => x.CustomerId == customerId)
                    .Where(x => x.Status == 1)
                    .Select(x => new User
                    {
                        Id = x.Id,
                        UserName = x.UserName,
                        Role = x.Role,
                        Email = x.Email,
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", customerId);
                    return null;
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} GetByIdAsync Function Error", typeof(UserRepository));
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

        public async Task<IEnumerable<User>> GetAllAsync(Expression<Func<User, bool>> predicate)
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
                _logger.LogError(ex, "{Repo} GetAllAsync Function Error", typeof(UserRepository));
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
                existingUser.UserName = user.Email;
                existingUser.UserName = user.Role;
                existingUser.UserName = user.CustomerId;
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
