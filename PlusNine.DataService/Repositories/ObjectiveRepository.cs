using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlusNine.DataService.Data;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;

namespace PlusNine.DataService.Repositories
{
    public class ObjectiveRepository : GenericRepository<Objective>, IObjectiveRepository
    {
        public ObjectiveRepository(AppDbContext context, ILogger logger) : base(context, logger)
        {
        }

        public override async Task<IEnumerable<Objective>> All()
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
                _logger.LogError(ex, "{Repo} All Function Error", typeof(ObjectiveRepository));
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

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} Delete Function Error", typeof(ObjectiveRepository));
                throw;
            }
        }

        public override async Task<bool> Update(Objective objective)
        {
            try
            {
                var result = await _dbSet.FirstOrDefaultAsync(x => x.Id == objective.Id);

                if (result == null)
                {
                    return false;
                }

                result.UpdatedDate = DateTime.Now;
                result.ObjectiveName = objective.ObjectiveName;
                result.CurrentAmount = objective.CurrentAmount;
                result.AmountToComplete = objective.AmountToComplete;
                result.Progress = objective.Progress;
                result.Completed = objective.Completed;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Repo} Update Function Error", typeof(ObjectiveRepository));

                throw;
            }
        }
    }
}
