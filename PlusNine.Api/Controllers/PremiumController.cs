using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Logic;
using PlusNine.Logic.Interfaces;

namespace PlusNine.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]

    public class PremiumController : BaseController
    {
        private readonly IObjectiveService _objectiveService;

        public PremiumController(IUnitOfWork unitOfWork, IMapper mapper, IObjectiveService objectiveService)
        : base(unitOfWork, mapper)
        {
            _objectiveService = objectiveService;

        }
        [Authorize(Roles = "Premium")]
        [HttpGet("GetFriendObjective")]
        public async Task<IActionResult> GetAllFriendObjectives(Guid friendId)
        {
            var objectives = await _objectiveService.GetAllObjectives(friendId);
            return Ok(objectives);
        }
    }
}
