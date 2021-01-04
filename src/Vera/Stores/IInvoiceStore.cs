using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Audit;
using Vera.Models;

namespace Vera.Stores
{
    public interface IInvoiceStore
    {
        /// <summary>
        /// Persists the given invoice.
        /// </summary>
        /// <param name="invoice"></param>
        /// <param name="bucket"></param>
        /// <returns></returns>
        Task Save(Invoice invoice, string bucket);

        /// <summary>
        /// Returns the last invoice (if any) based on the given invoice. E.g. invoices may have there own
        /// sequence based their properties.
        /// </summary>
        /// <param name="bucket"></param>
        /// <returns></returns>
        Task<Invoice> Last(string bucket);

        IAsyncEnumerable<Invoice> List(AuditCriteria criteria);
    }
}