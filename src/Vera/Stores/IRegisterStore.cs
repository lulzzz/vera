using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    public interface IRegisterStore
    {
        Task Store(Register register);
        Task<Register> Get(Guid supplierId, Guid registerId);
        Task<Register> GetBySystemIdAndSupplierId(Guid supplierId, string systemId);
        Task<ICollection<Register>> GetOpenRegistersForSupplier(Guid supplierId);
        Task Update(Register register);
        Task<int> GetTotalRegisters(Guid supplierId);
    }
}
