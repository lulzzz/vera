using System;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    public interface IReportStore
    {
        Task Store(RegisterReport registerReport);
        Task<RegisterReport> GetByNumber(Guid accountId, string number);
        Task Delete(RegisterReport registerReport);
    }
}
