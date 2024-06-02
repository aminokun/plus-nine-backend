using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Requests;
using PlusNine.Entities.Dtos.Responses;
using PlusNine.Logic.Interfaces;

namespace PlusNine.Api.Controllers
{
    public class ObjectiveController : BaseController
    {
        private readonly IObjectiveService _objectiveService;

        public ObjectiveController(IUnitOfWork unitOfWork, IMapper mapper, IObjectiveService objectiveService)
            : base(unitOfWork, mapper)
        {
            _objectiveService = objectiveService;
        }

        [Authorize]
        [HttpGet]
        [Route("{objectiveId:guid}")]
        public async Task<IActionResult> GetObjective(Guid objectiveId)
        {
            try
            {
                var result = await _objectiveService.GetObjective(objectiveId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllObjectives()
        {
            var objectives = await _objectiveService.GetAllObjectives();
            return Ok(objectives);
        }

        [Authorize]
        [HttpPost("")]
        public async Task<IActionResult> AddObjective([FromBody] CreateObjectiveRequest objective)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var result = await _objectiveService.AddObjective(objective);
            return CreatedAtAction(nameof(GetObjective), new { objectiveId = result.Id }, result);
        }

        [Authorize]
        [HttpPut("")]
        public async Task<IActionResult> UpdateObjective([FromBody] UpdateObjectiveRequest objective)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            await _objectiveService.UpdateObjective(objective);
            return NoContent();
        }

        [Authorize]
        [HttpDelete]
        [Route("{objectiveId:guid}")]
        public async Task<IActionResult> DeleteObjective(Guid objectiveId)
        {
            try
            {
                await _objectiveService.DeleteObjective(objectiveId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
