using Microsoft.Extensions.Logging;
using PlusNine.DataService.Data;
using PlusNine.DataService.Repositories.Interfaces;

namespace PlusNine.DataService.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AppDbContext _context;
        public IObjectiveRepository Objectives { get; }
        public IUserRepository User { get; }
        public IFriendRequestRepository FriendRequest { get; }
        public IFriendshipRepository Friendship { get; }

        public UnitOfWork(AppDbContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            var logger = loggerFactory.CreateLogger("logs");

            Objectives = new ObjectiveRepository(context, logger);
            User = new UserRepository(context, logger);
            FriendRequest = new FriendRequestRepository(context, logger);
            Friendship = new FriendshipRepository(context, logger);
        }

        public async Task<bool> CompleteAsync()
        {
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
