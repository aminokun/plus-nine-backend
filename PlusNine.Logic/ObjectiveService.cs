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

        public async Task<GetObjectiveResponse> GetObjective(Guid objectiveId)
        {
            var objective = await _unitOfWork.Objectives.GetById(objectiveId) ?? throw new ArgumentException("Objective Not Found", nameof(objectiveId));
            var result = _mapper.Map<GetObjectiveResponse>(objective);

            return result;
        }

        public async Task<IEnumerable<Objective>> GetAllObjectives()
        {
            var objectives = await _unitOfWork.Objectives.All();
            return _mapper.Map<IEnumerable<Objective>>(objectives);
        }

        public async Task<Objective> AddObjective(CreateObjectiveRequest objective)
        {
            var result = _mapper.Map<Objective>(objective);

            await _unitOfWork.Objectives.Add(result);
            await _unitOfWork.CompleteAsync();

            return result;
        }

        public async Task UpdateObjective(UpdateObjectiveRequest objective)
        {
            var result = _mapper.Map<Objective>(objective);

            await _unitOfWork.Objectives.Update(result);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteObjective(Guid objectiveId)
        {
            var objective = await _unitOfWork.Objectives.GetById(objectiveId);

            if (objective == null)
                throw new ArgumentException("Objective Not Found", nameof(objectiveId));

            await _unitOfWork.Objectives.Delete(objectiveId);
            await _unitOfWork.CompleteAsync();
        }
    }
}
