using System;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    public interface ICompanyStore
    {
        Task<Company> Store(Company company);
        Task<Company> Update(Company company);
        Task<Company> GetByName(string name);
        Task<Company> Get(Guid companyId);
    }
}