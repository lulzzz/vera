using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    public interface IAccountStore
    {
        Task Store(Account account);
        Task Update(Account account);
        Task<Account> Get(Guid companyId, Guid accountId);
        Task<ICollection<Account>> GetByCompany(Guid companyId);
    }
}