using AutoMapper;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Requests;
using PlusNine.Entities.Dtos.Responses;
using PlusNine.Logic.Interfaces;

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
    }
}
