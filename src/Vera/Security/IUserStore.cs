using System;
using System.Threading.Tasks;

namespace Vera.Security
{
    public interface IUserStore
    {
        Task<User> Store(User user);
        Task Update(User user);
        Task<User> GetByCompany(Guid companyId, string username);
    }
}