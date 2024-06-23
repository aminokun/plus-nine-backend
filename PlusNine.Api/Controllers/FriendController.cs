using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Logic.Interfaces;
using PlusNine.Entities.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;

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

        [Authorize]
        [HttpGet("Search")]
        public async Task<IActionResult> SearchUsers(string username)
        {
            try
            {
                var userResponses = await _friendService.SearchUsers(username);
                return Ok(userResponses);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
 
        [Authorize]
        [HttpGet("GetFriends")]
        public async Task<IActionResult> GetFriends()
        {
            try
            {
                var userId = GetUserId();
                var friendShips = await _friendService.GetFriends(userId);
                return Ok(friendShips);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("IncommingRequests")]
        public async Task<IActionResult> GetFriendRequests()
        {
            try
            {
                var userId = GetUserId();
                var friendRequests = await _friendService.GetFriendRequests(userId);
                return Ok(friendRequests);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [Authorize]
        [HttpPost("SendRequest")]
        public async Task<IActionResult> SendFriendRequest(Guid receiverId)
        {
            try
            {
                var userId = GetUserId();
                await _friendService.SendFriendRequest(userId, receiverId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("Accept/{requestId}")]
        public async Task<IActionResult> AcceptFriendRequest(Guid requestId)
        {
            try
            {
                await _friendService.AcceptFriendRequest(requestId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("Reject/{requestId}")]
        public async Task<IActionResult> RejectFriendRequest(Guid requestId)
        {
            try
            {
                await _friendService.RejectFriendRequest(requestId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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
