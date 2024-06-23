using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using PlusNine.DataService.Data;
using PlusNine.DataService.Repositories;
using PlusNine.Entities.DbSet;
using Xunit;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace PlusNine.IntegrationTests
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly UserRepository _userRepository;
        private readonly ILogger<UserRepository> _logger;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _logger = new Logger<UserRepository>(new LoggerFactory());
            _userRepository = new UserRepository(_context, _logger);
        }

        private User CreateTestUser(string userName = "testuser", string email = "test@example.com", string customerId = "cust123")
        {
            return new User
            {
                UserName = userName,
                Email = email,
                CustomerId = customerId,
                Role = "Member",
                PasswordSalt = new byte[64],
                PasswordHash = new byte[64],
                Status = 1
            };
        }

        [Fact]
        public async Task GetByIdAsync_ExistingUser_ReturnsUser()
        {
            // Arrange
            var user = CreateTestUser();
            await _context.User.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetByIdAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.UserName.Should().Be("testuser");
            result.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task All_ReturnsAllActiveUsers()
        {
            // Arrange
            var user1 = CreateTestUser("user1", "user1@example.com");
            var user2 = CreateTestUser("user2", "user2@example.com");
            var inactiveUser = CreateTestUser("inactive", "inactive@example.com");
            inactiveUser.Status = 0;

            await _context.User.AddRangeAsync(user1, user2, inactiveUser);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.All();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(u => u.UserName == "user1");
            result.Should().Contain(u => u.UserName == "user2");
            result.Should().NotContain(u => u.UserName == "inactive");
        }

        [Fact]
        public async Task Delete_ExistingUser_SetsStatusToZero()
        {
            // Arrange
            var user = CreateTestUser();
            await _context.User.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.Delete(user.Id);

            // Assert
            result.Should().BeTrue();
            var deletedUser = await _context.User.FindAsync(user.Id);
            deletedUser.Status.Should().Be(0);
        }

        [Fact]
        public async Task Update_ExistingUser_UpdatesUserDetails()
        {
            // Arrange
            var user = CreateTestUser();
            await _context.User.AddAsync(user);
            await _context.SaveChangesAsync();

            var updatedUser = new User
            {
                Id = user.Id,
                UserName = "updateduser",
                Email = "updated@example.com",
                Role = "Admin",
                CustomerId = "newcust123",
                PasswordSalt = new byte[64],
                PasswordHash = new byte[64],
            };

            // Act
            var result = await _userRepository.Update(updatedUser);

            // Assert
            result.Should().BeTrue();
            var retrievedUser = await _context.User.FindAsync(user.Id);
            retrievedUser.UserName.Should().Be("updateduser");
            retrievedUser.Email.Should().Be("updated@example.com");
            retrievedUser.Role.Should().Be("Admin");
            retrievedUser.CustomerId.Should().Be("newcust123");
        }

        //not happy flow
        [Fact]
        public async Task GetByIdAsync_NonExistentUser_ReturnsNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _userRepository.GetByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_InactiveUser_ReturnsNull()
        {
            // Arrange
            var inactiveUser = CreateTestUser();
            inactiveUser.Status = 0;
            await _context.User.AddAsync(inactiveUser);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetByIdAsync(inactiveUser.Id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByCustomerIdAsync_NonExistentCustomerId_ReturnsNull()
        {
            // Arrange
            var nonExistentCustomerId = "nonexistent123";

            // Act
            var result = await _userRepository.GetByCustomerIdAsync(nonExistentCustomerId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task All_NoActiveUsers_ReturnsEmptyList()
        {
            // Arrange
            var inactiveUser = CreateTestUser();
            inactiveUser.Status = 0;
            await _context.User.AddAsync(inactiveUser);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.All();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Delete_NonExistentUser_ReturnsFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _userRepository.Delete(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Update_NonExistentUser_ReturnsFalse()
        {
            // Arrange
            var nonExistentUser = CreateTestUser();
            nonExistentUser.Id = Guid.NewGuid(); // Ensure this ID doesn't exist in the database

            // Act
            var result = await _userRepository.Update(nonExistentUser);

            // Assert
            result.Should().BeFalse();
        }


        [Fact]
        public async Task GetAllAsync_WithPredicateNoMatch_ReturnsEmptyList()
        {
            // Arrange
            var user = CreateTestUser();
            await _context.User.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetAllAsync(u => u.Role == "NonExistentRole");

            // Assert
            result.Should().BeEmpty();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}