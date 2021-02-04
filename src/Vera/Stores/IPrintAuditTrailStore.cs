using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    public interface IPrintAuditTrailStore
    {
        Task<PrintTrail> Create(Guid invoiceId);
        Task Update(PrintTrail trail);
        Task<PrintTrail> Get(Guid invoiceId, Guid trailId);

        /// <summary>
        /// Returns all of the prints that were requested for the given invoice.
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <returns></returns>
        Task<ICollection<PrintTrail>> GetByInvoice(Guid invoiceId);
    }
}