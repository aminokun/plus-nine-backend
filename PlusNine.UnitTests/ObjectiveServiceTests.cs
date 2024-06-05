using AutoMapper;
using Moq;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Responses;
using PlusNine.Logic;
using PlusNine.Logic.MappingProfiles;
using Xunit;

namespace PlusNine.UnitTests
{
    public class ObjectiveServiceTests
    {
        private readonly IMapper _mapper;

        public ObjectiveServiceTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DomainToResponse>();
                cfg.AddProfile<RequestToDomain>();
            });

            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task GetAllObjectives_ReturnsUserSpecificObjectives()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();

            var objectiveService = new ObjectiveService(unitOfWorkMock.Object, _mapper);

            var userId = Guid.NewGuid();

            var objectives = new List<Objective>
            {
                new() { Id = Guid.NewGuid(), UserId = userId, ObjectiveName = "Objective 1" },
                new() { Id = Guid.NewGuid(), UserId = userId, ObjectiveName = "Objective 2" },
                new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), ObjectiveName = "Objective 3" }
            };

            unitOfWorkMock.Setup(uow => uow.Objectives.All()).ReturnsAsync(objectives);

            // Act
            var result = await objectiveService.GetAllObjectives(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, o => Assert.Equal(userId, o.UserId));
            Assert.IsType<List<GetObjectiveResponse>>(result);
        }
    }
}