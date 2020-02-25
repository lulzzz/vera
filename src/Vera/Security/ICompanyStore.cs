using System;
using System.Threading.Tasks;

namespace Vera.Security
{
    public interface ICompanyStore
    {
        Task Store(Company company);

        Task<Company> Get(Guid companyId);
    }
}