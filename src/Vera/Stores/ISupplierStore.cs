using System;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    public interface ISupplierStore
    {
        Task Store(Supplier supplier);
        Task Update(Supplier supplier);
        Task<Supplier> Get(Guid accountId, string systemId);
        Task Delete(Supplier supplier);
    }
}
