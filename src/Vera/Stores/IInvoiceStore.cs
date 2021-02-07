using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Audits;
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
        Task Store(Invoice invoice);

        /// <summary>
        /// Returns the invoice for the given account by its invoice number.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        Task<Invoice> GetByNumber(Guid accountId, string number);

        Task<ICollection<Invoice>> List(AuditCriteria criteria);

        Task Delete(Invoice invoice);
    }
}