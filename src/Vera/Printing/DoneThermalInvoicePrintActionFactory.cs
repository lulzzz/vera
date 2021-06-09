using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Printing
{
    /// <summary>
    /// Default implementation for the print action factory.
    /// </summary>
    public sealed class DoneThermalInvoicePrintActionFactory: IThermalInvoicePrintActionFactory
    {
        public Task<IPrintAction> Create(Account account, Invoice invoice)
        {
            return Task.FromResult<IPrintAction>(new DonePrintAction());
        }
    }
}
