using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Logic.Interfaces;
using PlusNine.Entities.Dtos.Requests;

namespace PlusNine.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FriendController : BaseController
    {
        private readonly IFriendService _friendService;

        public FriendController(IUnitOfWork unitOfWork, IMapper mapper, IFriendService friendService)
            : base(unitOfWork, mapper)
        {
            _friendService = friendService;
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchUsers(string username)
        {
            var userResponses = await _friendService.SearchUsers(username);
            return Ok(userResponses);
        }

        [HttpPost("Request")]
        public async Task<IActionResult> SendFriendRequest(SendFriendRequestRequest request)
        {
            await _friendService.SendFriendRequest(request);
            return Ok();
        }

        [HttpPut("Accept/{requestId}")]
        public async Task<IActionResult> AcceptFriendRequest(Guid requestId)
        {
            await _friendService.AcceptFriendRequest(requestId);
            return Ok();
        }

        [HttpPut("Reject/{requestId}")]
        public async Task<IActionResult> RejectFriendRequest(Guid requestId)
        {
            await _friendService.RejectFriendRequest(requestId);
            return Ok();
        }
    }
}
