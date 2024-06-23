using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Moq;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Entities.DbSet;
using PlusNine.Entities.Dtos.Responses;
using PlusNine.Logic;
using PlusNine.Logic.Hubs;
using Xunit;
using Xunit.Sdk;

namespace PlusNine.Tests
{
    public class FriendServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IHubContext<FriendHub>> _mockHubContext;
        private readonly FriendService _friendService;

        public FriendServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockHubContext = new Mock<IHubContext<FriendHub>>();
            _friendService = new FriendService(_mockUnitOfWork.Object, _mockMapper.Object, _mockHubContext.Object);
        }

        [Fact]
        public async Task GetFriendRequests_ReturnsCorrectFriendRequests()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var friendRequests = new List<FriendRequest>
            {
                new FriendRequest { Id = Guid.NewGuid(), SenderId = Guid.NewGuid(), ReceiverId = userId, FriendShipStatus = FriendRequestStatus.Pending },
                new FriendRequest { Id = Guid.NewGuid(), SenderId = Guid.NewGuid(), ReceiverId = userId, FriendShipStatus = FriendRequestStatus.Pending }
            };

            _mockUnitOfWork.Setup(uow => uow.FriendRequest.GetAllAsync(It.IsAny<Expression<Func<FriendRequest, bool>>>()))
                .ReturnsAsync(friendRequests);

            _mockUnitOfWork.Setup(uow => uow.User.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new User { UserName = "TestUser", Email = "test@example.com", Role = "Member", PasswordSalt = new byte[0], PasswordHash = new byte[0] });

            // Act
            var result = await _friendService.GetFriendRequests(userId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, fr => Assert.Equal("TestUser", fr.Username));
        }

        [Fact]
        public async Task GetFriends_ReturnsCorrectFriends()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var friendships = new List<Friendship>
            {
                new Friendship { User1Id = userId, User2Id = Guid.NewGuid() },
                new Friendship { User1Id = Guid.NewGuid(), User2Id = userId }
            };

            _mockUnitOfWork.Setup(uow => uow.Friendship.GetAllAsync(It.IsAny<Expression<Func<Friendship, bool>>>()))
                .ReturnsAsync(friendships);

            _mockUnitOfWork.Setup(uow => uow.User.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new User { Id = Guid.NewGuid(), UserName = "TestFriend", Email = "friend@example.com", Role = "Member", PasswordSalt = new byte[0], PasswordHash = new byte[0] });

            // Act
            var result = await _friendService.GetFriends(userId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, fr => Assert.Equal("TestFriend", fr.Username));
        }

        [Fact]
        public async Task SendFriendRequest_ThrowsException_WhenSendingToSelf()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _friendService.SendFriendRequest(userId, userId));
        }

        [Fact]
        public async Task SendFriendRequest_ThrowsException_WhenRequestAlreadyExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();

            _mockUnitOfWork.Setup(uow => uow.FriendRequest.SingleOrDefaultAsync(It.IsAny<Func<FriendRequest, bool>>()))
                .ReturnsAsync(new FriendRequest());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _friendService.SendFriendRequest(userId, receiverId));
        }

        [Fact]
        public async Task AcceptFriendRequest_UpdatesFriendRequestAndCreatesFriendship()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var friendRequest = new FriendRequest
            {
                Id = requestId,
                FriendShipStatus = FriendRequestStatus.Pending,
                SenderId = senderId
            };

            _mockUnitOfWork.Setup(uow => uow.FriendRequest.GetByIdAsync(requestId))
                .ReturnsAsync(friendRequest);

            _mockUnitOfWork.Setup(uow => uow.FriendRequest.Update(It.IsAny<FriendRequest>()))
                .ReturnsAsync(true);

            _mockUnitOfWork.Setup(uow => uow.Friendship.Add(It.IsAny<Friendship>()))
                .ReturnsAsync(true);

            _mockUnitOfWork.Setup(uow => uow.CompleteAsync())
                .ReturnsAsync(true);

            _mockMapper.Setup(m => m.Map<Friendship>(It.IsAny<FriendRequest>()))
                .Returns(new Friendship());

            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            mockClients.Setup(clients => clients.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
            _mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

            // Act
            await _friendService.AcceptFriendRequest(requestId);

            // Assert
            Assert.Equal(FriendRequestStatus.Accepted, friendRequest.FriendShipStatus);
            _mockUnitOfWork.Verify(uow => uow.FriendRequest.Update(It.IsAny<FriendRequest>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.Friendship.Add(It.IsAny<Friendship>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
            mockClientProxy.Verify(x => x.SendCoreAsync("FriendRequestAccepted", It.IsAny<object[]>(), default), Times.Once);
        }

        [Fact]
        public async Task RejectFriendRequest_UpdatesFriendRequest()
        {
            // Arrange
            var requestId = Guid.NewGuid();
            var senderId = Guid.NewGuid();
            var friendRequest = new FriendRequest
            {
                Id = requestId,
                FriendShipStatus = FriendRequestStatus.Pending,
                SenderId = senderId
            };

            _mockUnitOfWork.Setup(uow => uow.FriendRequest.GetByIdAsync(requestId))
                .ReturnsAsync(friendRequest);

            _mockUnitOfWork.Setup(uow => uow.FriendRequest.Update(It.IsAny<FriendRequest>()))
                .ReturnsAsync(true);

            _mockUnitOfWork.Setup(uow => uow.CompleteAsync())
                .ReturnsAsync(true);

            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            mockClients.Setup(clients => clients.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
            _mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

            // Act
            await _friendService.RejectFriendRequest(requestId);

            // Assert
            Assert.Equal(FriendRequestStatus.Rejected, friendRequest.FriendShipStatus);
            _mockUnitOfWork.Verify(uow => uow.FriendRequest.Update(It.IsAny<FriendRequest>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
            mockClientProxy.Verify(x => x.SendCoreAsync("FriendRequestRejected", It.IsAny<object[]>(), default), Times.Once);
        }

        [Fact]
        public async Task SearchUsers_ReturnsCorrectUsers()
        {
            // Arrange
            var username = "test";
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), UserName = "test1", Email = "test1@example.com", Role = "Member", PasswordSalt = new byte[0], PasswordHash = new byte[0] },
                new User { Id = Guid.NewGuid(), UserName = "test2", Email = "test2@example.com", Role = "Member", PasswordSalt = new byte[0], PasswordHash = new byte[0] }
            };

            _mockUnitOfWork.Setup(uow => uow.User.GetAllAsync(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(users);

            _mockMapper.Setup(m => m.Map<IEnumerable<UserSearchResponse>>(It.IsAny<IEnumerable<User>>()))
                .Returns(users.Select(u => new UserSearchResponse { Id = u.Id, UserName = u.UserName }));

            // Act
            var result = await _friendService.SearchUsers(username);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, u => u.UserName == "test1");
            Assert.Contains(result, u => u.UserName == "test2");
        }
    }
}