using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    public interface IGrandTotalAuditTrailStore
    {
        Task<GrandTotalAuditTrail> Create(Invoice invoice, decimal grandTotal);
        Task Delete(GrandTotalAuditTrail grandTotalAuditTrail);
    }
}
