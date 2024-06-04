﻿using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Requests;
using PlusNine.Entities.Dtos.Responses;

namespace PlusNine.Logic.Interfaces
{
    public interface IObjectiveService
    {
        Task<GetObjectiveResponse> GetObjective(Guid objectiveId, Guid userId);
        Task<IEnumerable<Objective>> GetAllObjectives(Guid userId);
        Task<Objective> AddObjective(CreateObjectiveRequest objective, Guid userId);
        Task UpdateObjective(UpdateObjectiveRequest objective, Guid userId);
        Task DeleteObjective(Guid objectiveId, Guid userId);
    }
}