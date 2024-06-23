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
    public class ObjectiveRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly ObjectiveRepository _repository;
        private readonly ILogger<ObjectiveRepository> _logger;

        public ObjectiveRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _logger = new Logger<ObjectiveRepository>(new LoggerFactory());
            _repository = new ObjectiveRepository(_context, _logger);
        }

        private Objective CreateTestObjective(string name = "Test Objective", int currentAmount = 50, int amountToComplete = 100)
        {
            return new Objective
            {
                UserId = Guid.NewGuid(),
                ObjectiveName = name,
                CurrentAmount = currentAmount,
                AmountToComplete = amountToComplete,
                Progress = (currentAmount * 100) / amountToComplete,
                Completed = false,
                Status = 1
            };
        }

        [Fact]
        public async Task All_ReturnsAllActiveObjectives()
        {
            // Arrange
            var objective1 = CreateTestObjective("Objective 1");
            var objective2 = CreateTestObjective("Objective 2");
            var inactiveObjective = CreateTestObjective("Inactive Objective");
            inactiveObjective.Status = 0;

            await _context.Objectives.AddRangeAsync(objective1, objective2, inactiveObjective);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.All();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(o => o.ObjectiveName == "Objective 1");
            result.Should().Contain(o => o.ObjectiveName == "Objective 2");
            result.Should().NotContain(o => o.ObjectiveName == "Inactive Objective");
        }

        [Fact]
        public async Task All_NoActiveObjectives_ReturnsEmptyList()
        {
            // Arrange
            var inactiveObjective = CreateTestObjective();
            inactiveObjective.Status = 0;
            await _context.Objectives.AddAsync(inactiveObjective);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.All();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Delete_ExistingObjective_SetsStatusToZero()
        {
            // Arrange
            var objective = CreateTestObjective();
            await _context.Objectives.AddAsync(objective);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.Delete(objective.Id);

            // Assert
            result.Should().BeTrue();
            var deletedObjective = await _context.Objectives.FindAsync(objective.Id);
            deletedObjective.Status.Should().Be(0);
        }

        [Fact]
        public async Task Delete_NonExistentObjective_ReturnsFalse()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.Delete(nonExistentId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Update_ExistingObjective_UpdatesObjectiveDetails()
        {
            // Arrange
            var objective = CreateTestObjective();
            await _context.Objectives.AddAsync(objective);
            await _context.SaveChangesAsync();

            var updatedObjective = new Objective
            {
                Id = objective.Id,
                ObjectiveName = "Updated Objective",
                CurrentAmount = 75,
                AmountToComplete = 100,
                Progress = 75,
                Completed = true
            };

            // Act
            var result = await _repository.Update(updatedObjective);

            // Assert
            result.Should().BeTrue();
            var retrievedObjective = await _context.Objectives.FindAsync(objective.Id);
            retrievedObjective.ObjectiveName.Should().Be("Updated Objective");
            retrievedObjective.CurrentAmount.Should().Be(75);
            retrievedObjective.AmountToComplete.Should().Be(100);
            retrievedObjective.Progress.Should().Be(75);
            retrievedObjective.Completed.Should().BeTrue();
        }

        [Fact]
        public async Task Update_NonExistentObjective_ReturnsFalse()
        {
            // Arrange
            var nonExistentObjective = CreateTestObjective();
            nonExistentObjective.Id = Guid.NewGuid();

            // Act
            var result = await _repository.Update(nonExistentObjective);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetByIdAsync_ExistingObjective_ReturnsObjective()
        {
            // Arrange
            var objective = CreateTestObjective();
            await _context.Objectives.AddAsync(objective);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(objective.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(objective.Id);
            result.ObjectiveName.Should().Be(objective.ObjectiveName);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentObjective_ReturnsNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.GetByIdAsync(nonExistentId);

            // Assert
            result.Should().BeNull();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}