using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Requests;
using PlusNine.Entities.Dtos.Responses;
using PlusNine.Logic;
using PlusNine.Logic.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace PlusNine.UnitTests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IOptions<AppSettings>> _mockOptions;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockOptions = new Mock<IOptions<AppSettings>>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Provide a sufficiently long secret (at least 64 bytes for HMACSHA512)
            var longSecret = "a-very-long-secret-key-that-is-at-least-64-bytes-long-for-hmacsha512-algorithm";
            _mockOptions.Setup(x => x.Value).Returns(new AppSettings { Secret = longSecret });

            _authService = new AuthService(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockOptions.Object,
                _mockHttpContextAccessor.Object);
        }

        [Fact]
        public async Task Register_ValidUser_ReturnsUserResponse()
        {
            // Arrange
            var createUserRequest = new CreateUserRequest
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            _mockUnitOfWork.Setup(uow => uow.User.SingleOrDefaultAsync(It.IsAny<Func<User, bool>>()))
                .ReturnsAsync((User)null);

            _mockMapper.Setup(m => m.Map<User>(It.IsAny<CreateUserRequest>()))
                .Returns(new User { UserName = createUserRequest.UserName, Email = createUserRequest.Email, Role = "Member"});

            _mockMapper.Setup(m => m.Map<GetUserResponse>(It.IsAny<User>()))
                .Returns(new GetUserResponse { Username = createUserRequest.UserName, Email = createUserRequest.Email, Role = "Member" });

            // Act
            var result = await _authService.Register(createUserRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createUserRequest.UserName, result.Username);
            Assert.Equal(createUserRequest.Email, result.Email);
        }

        [Fact]
        public async Task Register_PasswordsDontMatch_ThrowsArgumentException()
        {
            // Arrange
            var createUserRequest = new CreateUserRequest
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "password123",
                ConfirmPassword = "password456"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _authService.Register(createUserRequest));
        }

        [Fact]
        public async Task Register_UserAlreadyExists_ThrowsArgumentException()
        {
            // Arrange
            var createUserRequest = new CreateUserRequest
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "password123",
                ConfirmPassword = "password123"
            };

            _mockUnitOfWork.Setup(uow => uow.User.SingleOrDefaultAsync(It.IsAny<Func<User, bool>>()))
                .ReturnsAsync(new User
                {
                    UserName = "testuser",
                    Email = "test@example.com",
                    Role = "Member"
                });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _authService.Register(createUserRequest));
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsJwtResponse()
        {
            // Arrange
            var loginRequest = new Login { UserName = "testuser", Password = "password123" };

            // Create a proper password hash
            byte[] passwordHash, passwordSalt;
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(loginRequest.Password));
            }

            var user = new User
            {
                UserName = "testuser",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Email = "test@example.com",
                Role = "User"
            };

            _mockUnitOfWork.Setup(uow => uow.User.SingleOrDefaultAsync(It.IsAny<Func<User, bool>>()))
                .ReturnsAsync(user);

            var mockHttpContext = new Mock<HttpContext>();
            var mockResponse = new Mock<HttpResponse>();
            var mockResponseCookies = new Mock<IResponseCookies>();
            mockResponse.Setup(r => r.Cookies).Returns(mockResponseCookies.Object);
            mockHttpContext.Setup(c => c.Response).Returns(mockResponse.Object);
            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(mockHttpContext.Object);

            // Act
            var result = await _authService.Login(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.UserName, result.Username);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.Role, result.Role);
            Assert.NotNull(result.Token);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var loginRequest = new Login { UserName = "testuser", Password = "wrongpassword" };

            _mockUnitOfWork.Setup(uow => uow.User.SingleOrDefaultAsync(It.IsAny<Func<User, bool>>()))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(loginRequest));
        }

        [Fact]
        public async Task Logout_RemovesCookies()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockResponse = new Mock<HttpResponse>();
            var mockResponseCookies = new Mock<IResponseCookies>();
            mockResponse.Setup(r => r.Cookies).Returns(mockResponseCookies.Object);
            mockHttpContext.Setup(c => c.Response).Returns(mockResponse.Object);
            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(mockHttpContext.Object);

            // Act
            await _authService.Logout();

            // Assert
            mockResponseCookies.Verify(c => c.Delete("X-Access-Token", It.IsAny<CookieOptions>()), Times.Once);
            mockResponseCookies.Verify(c => c.Delete("X-Refresh-Token", It.IsAny<CookieOptions>()), Times.Once);
        }


        [Fact]
        public async Task RefreshToken_ExpiredToken_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var refreshToken = "expired-refresh-token";
            var user = new User
            {
                UserName = "testuser",
                Email = "test@example.com",
                Role = "User",
                Token = refreshToken,
                TokenExpires = DateTime.Now.AddDays(-1)
            };

            _mockUnitOfWork.Setup(uow => uow.User.SingleOrDefaultAsync(It.IsAny<Func<User, bool>>()))
                .ReturnsAsync(user);

            var mockHttpContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            var mockRequestCookies = new Mock<IRequestCookieCollection>();
            mockRequestCookies.Setup(c => c["X-Refresh-Token"]).Returns(refreshToken);
            mockRequest.Setup(r => r.Cookies).Returns(mockRequestCookies.Object);
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(mockHttpContext.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.RefreshToken());
        }

        [Fact]
        public async Task JwtCheck_ValidToken_ReturnsUserResponse()
        {
            // Arrange
            var jwtToken = "valid-jwt-token";
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com",
                Role = "User"
            };

            var mockHttpContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            var mockRequestCookies = new Mock<IRequestCookieCollection>();
            mockRequestCookies.Setup(c => c["X-Access-Token"]).Returns(jwtToken);
            mockRequest.Setup(r => r.Cookies).Returns(mockRequestCookies.Object);
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);

            var claims = new[]
            {
                new Claim("Id", userId.ToString()),
                new Claim("username", user.UserName)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            mockHttpContext.Setup(c => c.User).Returns(claimsPrincipal);

            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(mockHttpContext.Object);

            _mockUnitOfWork.Setup(uow => uow.User.SingleOrDefaultAsync(It.IsAny<Func<User, bool>>()))
                .ReturnsAsync(user);

            _mockMapper.Setup(m => m.Map<GetUserResponse>(It.IsAny<User>()))
                .Returns(new GetUserResponse { Username = user.UserName, Email = user.Email, Role = user.Role});

            // Act
            var result = await _authService.JwtCheck();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.UserName, result.Username);
            Assert.Equal(user.Email, result.Email);
        }

        [Fact]
        public async Task JwtCheck_InvalidToken_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            var mockRequestCookies = new Mock<IRequestCookieCollection>();
            mockRequestCookies.Setup(c => c["X-Access-Token"]).Returns((string)null);
            mockRequest.Setup(r => r.Cookies).Returns(mockRequestCookies.Object);
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(mockHttpContext.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.JwtCheck());
        }
    }
}