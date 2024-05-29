using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Requests;
using PlusNine.Entities.Dtos.Responses;

namespace PlusNine.Api.Controllers
{
    public class ObjectiveController : BaseController
    {
        public ObjectiveController(
            IUnitOfWork unitOfWork, 
            IMapper mapper) : base(unitOfWork, mapper)
        {
        }
        [HttpGet]
        [Route("{objectiveId:guid}")]
        public async Task<IActionResult> GetObjective(Guid objectiveId)
        {
            var objective = await _unitOfWork.Objectives.GetById(objectiveId);

            if(objective == null)
                return NotFound("Objective Not Found");

            var result = _mapper.Map<GetObjectiveResponse>(objective);
            
            return Ok(result);
        }

        [Authorize]

        [HttpGet]
        public async Task<IActionResult> GetAllObjectives()
        {
            var objectives = await _unitOfWork.Objectives.All();
            return Ok(_mapper.Map<IEnumerable<Objective>>(objectives));
        }

        [HttpPost("")]
        public async Task<IActionResult> AddObjective([FromBody] CreateObjectiveRequest objective)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var result = _mapper.Map<Objective>(objective);

            await _unitOfWork.Objectives.Add(result);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(nameof(GetObjective), new { objectiveId = result.Id }, result);
        }

        [HttpPut("")]
        public async Task<IActionResult> UpdateObjective([FromBody] UpdateObjectiveRequest objective)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            else
            {
                var result = _mapper.Map<Objective>(objective);

                await _unitOfWork.Objectives.Update(result);
                await _unitOfWork.CompleteAsync();

                return NoContent();
            }
        }

        [HttpDelete]
        [Route("{objectiveId:guid}")]
        public async Task<IActionResult> DeleteObjective(Guid objectiveId)
        {
            var objective = await _unitOfWork.Objectives.GetById(objectiveId);

            if(objective == null)
                return NotFound();

            await _unitOfWork.Objectives.Delete(objectiveId);
            await _unitOfWork.CompleteAsync();
            
            return NoContent();
        }
    }
}
