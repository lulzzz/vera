using System;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Periods
{
    public interface IPeriodOpener
    {
        Task<Period> Open(Guid supplierId);
    }
}
