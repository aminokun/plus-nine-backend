using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.Dtos.Requests;
using PlusNine.Logic.Interfaces;
using PlusNine.Logic.Models;

namespace PlusNine.Api.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IUnitOfWork unitOfWork, IMapper mapper, IAuthService authService)
            : base(unitOfWork, mapper)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] CreateUserRequest model)
        {
            try
            {
                var userResponse = await _authService.Register(model);
                return Ok(userResponse);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            try
            {
                var tokenResponse = await _authService.Login(model);
                return Ok(tokenResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var tokenResponse = await _authService.RefreshToken();
                return Ok(tokenResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("JwtCheck")]
        public async Task<IActionResult> JwtCheck()
        {
            try
            {
                var userResponse = await _authService.JwtCheck();
                return Ok(userResponse);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [HttpDelete("RevokeToken")]
        public async Task<IActionResult> RevokeToken(string username)
        {
            try
            {
                await _authService.RevokeToken(username);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}