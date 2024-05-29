using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlusNine.DataService.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IObjectiveRepository Objectives { get; }
        IUserRepository User { get; }

        Task<bool> CompleteAsync();
    }
}
