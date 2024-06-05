using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.Dtos.Requests;
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
                var userId = GetUserId();
                var result = await _objectiveService.GetObjective(objectiveId, userId);
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
            var userId = GetUserId();
            var objectives = await _objectiveService.GetAllObjectives(userId);
            return Ok(objectives);
        }

        [Authorize]
        [HttpPost("")]
        public async Task<IActionResult> AddObjective([FromBody] CreateObjectiveRequest objective)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var userId = GetUserId();
            var result = await _objectiveService.AddObjective(objective, userId);
            return CreatedAtAction(nameof(GetObjective), new { objectiveId = result.Id }, result);
        }

        [Authorize]
        [HttpPut("")]
        public async Task<IActionResult> UpdateObjective([FromBody] UpdateObjectiveRequest objective)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var userId = GetUserId();
            await _objectiveService.UpdateObjective(objective, userId);
            return NoContent();
        }

        [Authorize]
        [HttpDelete]
        [Route("{objectiveId:guid}")]
        public async Task<IActionResult> DeleteObjective(Guid objectiveId)
        {
            try
            {
                var userId = GetUserId();
                await _objectiveService.DeleteObjective(objectiveId, userId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "Id");
            if (string.IsNullOrEmpty(userIdClaim?.Value))
                throw new UnauthorizedAccessException("User ID not found in claims.");

            return Guid.Parse(userIdClaim?.Value);
        }
    }
}