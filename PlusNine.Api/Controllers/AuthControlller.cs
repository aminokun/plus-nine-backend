using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PlusNine.Api.Models;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Requests;
using PlusNine.Entities.Dtos.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PlusNine.Api.Controllers
{
    public class AuthController : BaseController
    {
        private readonly AppSettings _applicationSettings;

        public AuthController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IOptions<AppSettings> applicationSettings) : base(unitOfWork, mapper)
        {
            _applicationSettings = applicationSettings.Value;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] Login model)
        {
            var user = await _unitOfWork.User.SingleOrDefaultAsync(u => u.UserName == model.UserName);
            if (user == null)
            {
                return BadRequest("Username or password was invalid");
            }

            var match = CheckPassword(model.Password, user);
            if (!match)
            {
                return BadRequest("Username or password was invalid");
            }

            var tokenResponse = await JWTGenerator(user);
            return Ok(tokenResponse);
        }

        private async Task<JwtResponse> JWTGenerator(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_applicationSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("username", user.UserName) }),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encryptedToken = tokenHandler.WriteToken(token);

            SetJWT(encryptedToken);

            var refreshToken = GenerateRefreshToken();
            await SetRefreshToken(refreshToken, user);

            return new JwtResponse
            {
                Token = encryptedToken,
                Username = user.UserName
            };
        }

        private static RefreshToken GenerateRefreshToken()
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(1),
                Created = DateTime.Now
            };
        }

        [HttpGet("RefreshToken")]
        public async Task<ActionResult<JwtResponse>> RefreshToken()
        {
            var refreshToken = Request.Cookies["X-Refresh-Token"];
            var user = await _unitOfWork.User.SingleOrDefaultAsync(u => u.Token == refreshToken);

            if (user == null || user.TokenExpires < DateTime.Now)
            {
                return Unauthorized("Token has expired");
            }

            var tokenResponse = await JWTGenerator(user);
            return Ok(tokenResponse);
        }

        private async Task SetRefreshToken(RefreshToken refreshToken, User user)
        {
            HttpContext.Response.Cookies.Append("X-Refresh-Token", refreshToken.Token,
                new CookieOptions
                {
                    Expires = refreshToken.Expires,
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                });

            user.Token = refreshToken.Token;
            user.TokenCreated = refreshToken.Created;
            user.TokenExpires = refreshToken.Expires;

            await _unitOfWork.User.Update(user);
            await _unitOfWork.CompleteAsync();
        }

        private void SetJWT(string encryptedToken)
        {
            HttpContext.Response.Cookies.Append("X-Access-Token", encryptedToken,
                new CookieOptions
                {
                    Expires = DateTime.Now.AddMinutes(15),
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                });
        }

        [HttpDelete("RevokeToken")]
        public async Task<IActionResult> RevokeToken(string username)
        {
            var user = await _unitOfWork.User.SingleOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.Token = string.Empty;
            user.TokenCreated = DateTime.MinValue;
            user.TokenExpires = DateTime.MinValue;

            var updateResult = await _unitOfWork.User.Update(user);
            if (!updateResult)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating user");
            }

            await _unitOfWork.CompleteAsync();

            return Ok();
        }

        private static bool CheckPassword(string password, User user)
        {
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(user.PasswordHash);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] CreateUserRequest model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest("Passwords don't match");
            }

            var user = _mapper.Map<User>(model);
            using (var hmac = new HMACSHA512())
            {
                user.PasswordSalt = hmac.Key;
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(model.Password));
            }

            await _unitOfWork.User.Add(user);
            await _unitOfWork.CompleteAsync();

            var userResponse = _mapper.Map<GetUserResponse>(user);

            return Ok(userResponse);
        }
    }
}
