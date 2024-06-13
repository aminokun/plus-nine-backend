using PlusNine.Logic.Models;
using PlusNine.Entities.Dtos.Responses;
using PlusNine.Entities.Dtos.Requests;

namespace PlusNine.Logic.Interfaces
{
    public interface IAuthService
    {
        Task<JwtResponse> Login(Login model);
        Task Logout();
        Task<JwtResponse> RefreshToken();
        Task RevokeToken(string username);
        Task<GetUserResponse> JwtCheck();
        Task<GetUserResponse> Register(CreateUserRequest model);
    }
}
