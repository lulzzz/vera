using System;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    public interface ICompanyStore
    {
        Task Store(Company company);
        Task Update(Company company);
        Task<Company> GetById(Guid companyId);
        Task<Company> GetByName(string name);
    }
}