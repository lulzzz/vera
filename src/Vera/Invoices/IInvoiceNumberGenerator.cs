using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Invoices
{
    public interface IInvoiceNumberGenerator
    {
        /// <summary>
        /// Generates an invoice number for the given invoice.
        /// </summary>
        /// <param name="invoice">Invoice to generate the invoice number for.</param>
        /// <returns>Invoice number based on the given invoice</returns>
        Task<string> Generate(Invoice invoice);
    }
}