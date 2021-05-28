using Vera.Models;

namespace Vera.Printing
{
    /// <summary>
    /// Default implementation for the print action factory.
    /// </summary>
    public sealed class DoneThermalInvoicePrintActionFactory: IThermalInvoicePrintActionFactory
    {
        public IPrintAction Create(Account account, Invoice invoice)
        {
            return new DonePrintAction();
        }
    }
}