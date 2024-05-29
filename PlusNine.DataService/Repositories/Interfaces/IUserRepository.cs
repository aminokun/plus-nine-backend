﻿using PlusNine.Entities.DbSet;


namespace PlusNine.DataService.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> SingleOrDefaultAsync(Func<User, bool> predicate);
    }
}