using System;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    public interface IPeriodStore
    {
        Task Store(Period period);
        Task<Period> Get(Guid periodId, Guid supplierId);
        Task<Period> GetOpenPeriodForSupplier(Guid supplierId);
        Task Update(Period period);
    }
}
