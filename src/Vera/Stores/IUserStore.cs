using System;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    public interface IUserStore
    {
        Task Store(User user);
        Task Update(User user);
        Task<User> GetByCompany(Guid companyId, string username);
    }
}