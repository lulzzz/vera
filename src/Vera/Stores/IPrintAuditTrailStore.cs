using System;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    public interface IPrintAuditTrailStore
    {
        Task<PrintTrail> Create(Guid invoiceId);
        Task<PrintTrail> Get(Guid invoiceId, Guid trailId);
        Task Update(PrintTrail trail);
    }
}