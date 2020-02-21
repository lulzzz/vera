using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    public interface IInvoiceStore
    {
        /// <summary>
        /// Persists the given invoice.
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns></returns>
        Task Save(Invoice invoice);

        /// <summary>
        /// Returns the last invoice (if any) based on the given invoice. E.g. invoices may have there own
        /// sequence based their properties.
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        Task<Invoice> Last(string sequence);
    }
}