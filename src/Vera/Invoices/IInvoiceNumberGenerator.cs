using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Invoices
{
    public interface IInvoiceNumberGenerator
    {
        Task<string> Generate(Invoice invoice, Invoice last);
    }
}