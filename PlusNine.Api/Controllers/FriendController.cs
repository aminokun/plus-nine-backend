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

        [HttpGet("requests")]
        public async Task<IActionResult> GetFriendRequests()
        {
            var userId = GetUserId();
            var friendRequests = await _friendService.GetFriendRequests(userId);
            return Ok(friendRequests);
        }

        [HttpPost("Request")]
        public async Task<IActionResult> SendFriendRequest(Guid receiverId)
        {
            var userId = GetUserId();
            await _friendService.SendFriendRequest(userId, receiverId);
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

        private Guid GetUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "Id");
            if (string.IsNullOrEmpty(userIdClaim?.Value))
                throw new UnauthorizedAccessException("User ID not found in claims.");

            return Guid.Parse(userIdClaim?.Value);
        }
    }
}
