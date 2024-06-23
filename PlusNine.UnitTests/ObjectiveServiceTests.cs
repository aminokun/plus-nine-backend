using AutoMapper;
using Moq;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Requests;
using PlusNine.Entities.Dtos.Responses;
using PlusNine.Logic;
using PlusNine.Logic.MappingProfiles;
using PlusNine.Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PlusNine.UnitTests
{
    public class ObjectiveServiceTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ObjectiveService _objectiveService;

        public ObjectiveServiceTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DomainToResponse>();
                cfg.AddProfile<RequestToDomain>();
            });

            _mapper = config.CreateMapper();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _objectiveService = new ObjectiveService(_unitOfWorkMock.Object, _mapper);
        }

        [Fact]
        public async Task GetAllObjectives_ReturnsUserSpecificObjectives()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var objectives = new List<Objective>
            {
                new() { Id = Guid.NewGuid(), UserId = userId, ObjectiveName = "Objective 1", Completed = false },
                new() { Id = Guid.NewGuid(), UserId = userId, ObjectiveName = "Objective 2", Completed = false },
                new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), ObjectiveName = "Objective 3", Completed = false }
            };

            _unitOfWorkMock.Setup(uow => uow.Objectives.All()).ReturnsAsync(objectives);

            // Act
            var result = await _objectiveService.GetAllObjectives(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, o => Assert.Equal(userId, o.UserId));
            Assert.IsType<List<GetObjectiveResponse>>(result);
        }

        [Fact]
        public async Task GetObjective_ReturnsCorrectObjective()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var objectiveId = Guid.NewGuid();
            var objective = new Objective { Id = objectiveId, UserId = userId, ObjectiveName = "Test Objective" };

            _unitOfWorkMock.Setup(uow => uow.Objectives.SingleOrDefaultAsync(It.IsAny<Func<Objective, bool>>()))
                .ReturnsAsync(objective);

            // Act
            var result = await _objectiveService.GetObjective(objectiveId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(objectiveId, result.ObjectiveId);
            Assert.Equal(userId, result.UserId);
            Assert.Equal("Test Objective", result.ObjectiveName);
        }

        [Fact]
        public async Task AddObjective_AddsObjectiveSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var createRequest = new CreateObjectiveRequest { ObjectiveName = "New Objective" };

            _unitOfWorkMock.Setup(uow => uow.Objectives.Add(It.IsAny<Objective>())).ReturnsAsync(true);
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(true);

            // Act
            var result = await _objectiveService.AddObjective(createRequest, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal("New Objective", result.ObjectiveName);
            _unitOfWorkMock.Verify(uow => uow.Objectives.Add(It.IsAny<Objective>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateObjective_UpdatesObjectiveSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var objectiveId = Guid.NewGuid();
            var updateRequest = new UpdateObjectiveRequest { ObjectiveId = objectiveId, ObjectiveName = "Updated Objective" };
            var existingObjective = new Objective { Id = objectiveId, UserId = userId, ObjectiveName = "Original Objective" };

            _unitOfWorkMock.Setup(uow => uow.Objectives.SingleOrDefaultAsync(It.IsAny<Func<Objective, bool>>()))
                .ReturnsAsync(existingObjective);
            _unitOfWorkMock.Setup(uow => uow.Objectives.Update(It.IsAny<Objective>())).ReturnsAsync(true);
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(true);

            // Act
            await _objectiveService.UpdateObjective(updateRequest, userId);

            // Assert
            Assert.Equal("Updated Objective", existingObjective.ObjectiveName);
            _unitOfWorkMock.Verify(uow => uow.Objectives.Update(It.IsAny<Objective>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteObjective_DeletesObjectiveSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var objectiveId = Guid.NewGuid();
            var existingObjective = new Objective { Id = objectiveId, UserId = userId, ObjectiveName = "Objective to Delete" };

            _unitOfWorkMock.Setup(uow => uow.Objectives.SingleOrDefaultAsync(It.IsAny<Func<Objective, bool>>()))
                .ReturnsAsync(existingObjective);
            _unitOfWorkMock.Setup(uow => uow.Objectives.Delete(objectiveId)).ReturnsAsync(true);
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(true);

            // Act
            await _objectiveService.DeleteObjective(objectiveId, userId);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.Objectives.Delete(objectiveId), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetCompletedObjectivesActivityCalendar_ReturnsCorrectCalendarData()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 1, 7);
            var objectives = new List<Objective>
            {
                new() { Id = Guid.NewGuid(), UserId = userId, ObjectiveName = "Obj 1", Completed = true, AddedDate = new DateTime(2023, 1, 1) },
                new() { Id = Guid.NewGuid(), UserId = userId, ObjectiveName = "Obj 2", Completed = true, AddedDate = new DateTime(2023, 1, 1) },
                new() { Id = Guid.NewGuid(), UserId = userId, ObjectiveName = "Obj 3", Completed = true, AddedDate = new DateTime(2023, 1, 3) },
                new() { Id = Guid.NewGuid(), UserId = userId, ObjectiveName = "Obj 4", Completed = false, AddedDate = new DateTime(2023, 1, 5) },
            };

            _unitOfWorkMock.Setup(uow => uow.Objectives.All()).ReturnsAsync(objectives);

            // Act
            var result = await _objectiveService.GetCompletedObjectivesActivityCalendar(userId, startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(7, result.Count());
            Assert.Equal(2, result.First(e => e.Date == "2023-01-01").Count);
            Assert.Equal(1, result.First(e => e.Date == "2023-01-03").Count);
            Assert.All(result.Where(e => e.Date != "2023-01-01" && e.Date != "2023-01-03"), e => Assert.Equal(0, e.Count));
        }
        [Fact]
        public async Task GetObjective_NonExistentObjective_ThrowsArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var objectiveId = Guid.NewGuid();

            _unitOfWorkMock.Setup(uow => uow.Objectives.SingleOrDefaultAsync(It.IsAny<Func<Objective, bool>>()))
                .ReturnsAsync((Objective)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _objectiveService.GetObjective(objectiveId, userId));
        }


        [Fact]
        public async Task UpdateObjective_NonExistentObjective_ThrowsArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var objectiveId = Guid.NewGuid();
            var updateRequest = new UpdateObjectiveRequest { ObjectiveId = objectiveId, ObjectiveName = "Updated Objective" };

            _unitOfWorkMock.Setup(uow => uow.Objectives.SingleOrDefaultAsync(It.IsAny<Func<Objective, bool>>()))
                .ReturnsAsync((Objective)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _objectiveService.UpdateObjective(updateRequest, userId));
        }

        [Fact]
        public async Task DeleteObjective_NonExistentObjective_ThrowsArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var objectiveId = Guid.NewGuid();

            _unitOfWorkMock.Setup(uow => uow.Objectives.SingleOrDefaultAsync(It.IsAny<Func<Objective, bool>>()))
                .ReturnsAsync((Objective)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _objectiveService.DeleteObjective(objectiveId, userId));
        }

        [Fact]
        public async Task AddObjective_DatabaseOperationFails_ThrowsException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var createRequest = new CreateObjectiveRequest { ObjectiveName = "New Objective" };

            _unitOfWorkMock.Setup(uow => uow.Objectives.Add(It.IsAny<Objective>())).ReturnsAsync(true);
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _objectiveService.AddObjective(createRequest, userId));
        }

        [Fact]
        public async Task GetAllObjectives_NoObjectivesExist_ReturnsEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _unitOfWorkMock.Setup(uow => uow.Objectives.All()).ReturnsAsync(new List<Objective>());

            // Act
            var result = await _objectiveService.GetAllObjectives(userId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCompletedObjectivesActivityCalendar_NoCompletedObjectives_ReturnsEmptyCalendar()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 1, 7);
            _unitOfWorkMock.Setup(uow => uow.Objectives.All()).ReturnsAsync(new List<Objective>());

            // Act
            var result = await _objectiveService.GetCompletedObjectivesActivityCalendar(userId, startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(7, result.Count());
            Assert.All(result, e => Assert.Equal(0, e.Count));
        }
    }
}