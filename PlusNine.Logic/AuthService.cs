using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PlusNine.Logic.Models;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Requests;
using PlusNine.Entities.Dtos.Responses;
using PlusNine.Logic.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PlusNine.Logic
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly AppSettings _applicationSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IUnitOfWork unitOfWork, IMapper mapper, IOptions<AppSettings> applicationSettings, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _applicationSettings = applicationSettings.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<GetUserResponse> Register(CreateUserRequest model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                throw new ArgumentException("Passwords don't match");
            }

            var existingUser = await _unitOfWork.User.SingleOrDefaultAsync(u => u.UserName == model.UserName);
            if (existingUser != null)
            {
                throw new ArgumentException("Username is already taken");
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
            return userResponse;
        }


        public async Task<JwtResponse> Login(Login model)
        {
            var user = await _unitOfWork.User.SingleOrDefaultAsync(u => u.UserName == model.UserName);
            if (user == null || !CheckPassword(model.Password, user))
            {
                throw new UnauthorizedAccessException("Username or password was invalid");
            }

            var tokenResponse = await JWTGenerator(user);
            return tokenResponse;
        }

        public async Task Logout()
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("X-Access-Token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });

            _httpContextAccessor.HttpContext.Response.Cookies.Delete("X-Refresh-Token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });
            
           await Task.CompletedTask;
        }

        public async Task<JwtResponse> RefreshToken()
        {
            var refreshToken = GetRefreshTokenFromCookie();
            var user = await _unitOfWork.User.SingleOrDefaultAsync(u => u.Token == refreshToken);

            if (user == null || user.TokenExpires < DateTime.Now)
            {
                throw new UnauthorizedAccessException("Token has expired");
            }

            var tokenResponse = await JWTGenerator(user);
            return tokenResponse;
        }

        public async Task RevokeToken(string username)
        {
            var user = await _unitOfWork.User.SingleOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            user.Token = string.Empty;
            user.TokenCreated = DateTime.MinValue;
            user.TokenExpires = DateTime.MinValue;

            var updateResult = await _unitOfWork.User.Update(user);
            if (!updateResult)
            {
                throw new Exception("Error updating user");
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task<GetUserResponse> JwtCheck()
        {
            var jwtToken = GetJwtTokenFromCookie();

            if (string.IsNullOrEmpty(jwtToken))
            {
                throw new UnauthorizedAccessException("JWT token not found");
            }

            var username = GetUsernameFromClaims();
            var Id = GetIdFromClaims();

            if (string.IsNullOrEmpty(username))
            {
                throw new UnauthorizedAccessException("Invalid JWT token");
            }

            var user = await _unitOfWork.User.SingleOrDefaultAsync(u => u.Id == Id && u.UserName == username) ?? throw new UnauthorizedAccessException("User not found");

            var userResponse = _mapper.Map<GetUserResponse>(user);
            return userResponse;
        }


        private async Task<JwtResponse> JWTGenerator(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_applicationSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { 
                    new Claim("Id", user.Id.ToString()),
                    new Claim("username", user.UserName),
                }),
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
                Expires = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };
        }

        private async Task SetRefreshToken(RefreshToken refreshToken, User user)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Append("X-Refresh-Token", refreshToken.Token,
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
            _httpContextAccessor.HttpContext.Response.Cookies.Append("X-Access-Token", encryptedToken,
                new CookieOptions
                {
                    Expires = DateTime.Now.AddHours(1),
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                });
        }

        private static bool CheckPassword(string password, User user)
        {
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(user.PasswordHash);
        }

        private string GetRefreshTokenFromCookie()
        {
            var refreshToken = _httpContextAccessor.HttpContext.Request.Cookies["X-Refresh-Token"];
            return refreshToken;
        }

        private string GetUsernameFromClaims()
        {
            var usernameClaim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "username");
            return usernameClaim?.Value;
        }
        private Guid GetIdFromClaims()
        {
            var IdClaim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id");
            Guid Id = Guid.Parse(IdClaim?.Value);
            return Id;
        }
        private string GetJwtTokenFromCookie()
        {
            var jwtToken = _httpContextAccessor.HttpContext.Request.Cookies["X-Access-Token"];
            return jwtToken;
        }
    }
}