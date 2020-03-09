using System;
using System.Threading.Tasks;

namespace Vera.Security
{
    public interface ICompanyStore
    {
        Task<Company> Store(Company company);
        Task<Company> Update(Company company);
        Task<Company> GetByName(string name);
        Task<Company> Get(Guid companyId);
    }
}