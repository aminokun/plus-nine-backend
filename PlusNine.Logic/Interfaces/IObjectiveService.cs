using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Requests;
using PlusNine.Entities.Dtos.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlusNine.Logic.Interfaces
{
    public interface IObjectiveService
    {
        Task<GetObjectiveResponse> GetObjective(Guid objectiveId);
        Task<IEnumerable<Objective>> GetAllObjectives();
        Task<Objective> AddObjective(CreateObjectiveRequest objective);
        Task UpdateObjective(UpdateObjectiveRequest objective);
        Task DeleteObjective(Guid objectiveId);
    }
}
