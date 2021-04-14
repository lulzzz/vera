using System;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    public interface IReportStore
    {
        Task Store(RegisterReport registerReport);
        Task<RegisterReport> Get(Guid registerReportId);
        Task Delete(RegisterReport registerReport);
    }
}
