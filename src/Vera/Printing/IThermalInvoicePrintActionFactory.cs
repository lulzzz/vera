using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Printing
{
    /// <summary>
    /// Factory that creates the chain of actions needed for print operations
    /// </summary>
    public interface IThermalInvoicePrintActionFactory
    {
        Task<IPrintAction> Create(Account account, Invoice invoice);
    }
}
