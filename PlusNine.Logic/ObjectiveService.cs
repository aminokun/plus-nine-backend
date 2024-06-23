using AutoMapper;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Requests;
using PlusNine.Entities.Dtos.Responses;
using PlusNine.Logic.Interfaces;
using PlusNine.Logic.Models;

namespace PlusNine.Logic
{
    public class ObjectiveService : IObjectiveService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ObjectiveService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<GetObjectiveResponse> GetObjective(Guid objectiveId, Guid userId)
        {
            var objective = await _unitOfWork.Objectives.SingleOrDefaultAsync(o => o.Id == objectiveId && o.UserId == userId);

            if (objective == null)
                throw new ArgumentException("Objective Not Found", nameof(objectiveId));

            var result = _mapper.Map<GetObjectiveResponse>(objective);

            return result;
        }

        public async Task<IEnumerable<GetObjectiveResponse>> GetAllObjectives(Guid userId)
        {
            var objectives = await _unitOfWork.Objectives.All();
            var userObjectives = objectives.Where(o => o.UserId == userId && o.Completed == false);
            return _mapper.Map<IEnumerable<GetObjectiveResponse>>(userObjectives);
        }

        public async Task<Objective> AddObjective(CreateObjectiveRequest objective, Guid userId)
        {
            var result = _mapper.Map<Objective>(objective);
            result.UserId = userId;

            await _unitOfWork.Objectives.Add(result);
            await _unitOfWork.CompleteAsync();

            return result;
        }

        public async Task<IEnumerable<GetObjectiveResponse>> GetCompletedObjectives(Guid userId)
        {
            var objectives = await _unitOfWork.Objectives.All();
            var userObjectives = objectives.Where(o => o.UserId == userId && o.Completed == true);
            return _mapper.Map<IEnumerable<GetObjectiveResponse>>(userObjectives);
        }


        public async Task UpdateObjective(UpdateObjectiveRequest objective, Guid userId)
        {
            var existingObjective = await _unitOfWork.Objectives.SingleOrDefaultAsync(o => o.Id == objective.ObjectiveId && o.UserId == userId);

            if (existingObjective == null)
                throw new ArgumentException("Objective Not Found", nameof(objective.ObjectiveId));

            _mapper.Map(objective, existingObjective);

            await _unitOfWork.Objectives.Update(existingObjective);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteObjective(Guid objectiveId, Guid userId)
        {
            var objective = await _unitOfWork.Objectives.SingleOrDefaultAsync(o => o.Id == objectiveId && o.UserId == userId);

            if (objective == null)
                throw new ArgumentException("Objective Not Found", nameof(objectiveId));

            await _unitOfWork.Objectives.Delete(objectiveId);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<ActivityCalendarEntry>> GetCompletedObjectivesActivityCalendar(Guid userId, DateTime startDate, DateTime endDate)
        {
            var objectives = await _unitOfWork.Objectives.All();
            var userCompletedObjectives = objectives
                .Where(o => o.UserId == userId && o.Completed && o.AddedDate >= startDate && o.AddedDate <= endDate)
                .ToList();

            var calendarData = new List<ActivityCalendarEntry>();

            var dateCounts = new Dictionary<string, int>();

            foreach (var obj in userCompletedObjectives)
            {
                var date = obj.AddedDate.ToString("yyyy-MM-dd");
                if (dateCounts.ContainsKey(date))
                {
                    dateCounts[date]++;
                }
                else
                {
                    dateCounts[date] = 1;
                }
            }

            int maxCount = dateCounts.Values.DefaultIfEmpty(0).Max();

            foreach (var date in EachDay(startDate, endDate))
            {
                var dateString = date.ToString("yyyy-MM-dd");
                dateCounts.TryGetValue(dateString, out int count);
                int level = CalculateLevel(count, maxCount);

                calendarData.Add(new ActivityCalendarEntry
                {
                    Count = count,
                    Date = dateString,
                    Level = level
                });
            }

            return calendarData;
        }

        private int CalculateLevel(int count, int maxCount)
        {
            if (maxCount == 0) return 0;
            double proportion = (double)count / maxCount;
            if (proportion > 0.75) return 4;
            if (proportion > 0.5) return 3;
            if (proportion > 0.25) return 2;
            if (proportion > 0) return 1;
            return 0;
        }

        private IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
        {
            for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
                yield return day;
        }

    }
}
