using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    public interface IRegisterStore
    {
        Task Store(Register register);
        Task<Register> Get(Guid registerId, Guid supplierId);
        Task<ICollection<Register>> GetOpenRegistersForSupplier(Guid supplierId);
        Task<ICollection<Register>> GetRegistersBasedOnSupplier(IEnumerable<Guid> registersIds, Guid supplierId);
        Task Update(Register register);
    }
}
