using System.Linq;
using Vera.Models;
using Vera.Portugal.Models;

namespace Vera.Portugal
{
    public static class InvoiceTypeHelper
    {
        public const decimal FaturaInvoiceLimit = 1000m;
        
        public static InvoiceType DetermineType(Invoice invoice)
        {
            // Ignore warning - want this to default hard to be explicit
            var invoiceType = InvoiceType.FT;

            var invoiceTotalAmount = invoice.Totals.Gross;

            if (invoiceTotalAmount < FaturaInvoiceLimit && invoice.Customer == null)
            {
                // Invoice receipt because it's an anonymous order
                invoiceType = InvoiceType.FR;
            }
            else
            {
                // Customer is available, so we have 2 options
                // FS (fatura simplificado) or FT (invoice)
                // FS => NIF available, below 1000 euros and in-store transaction
                // FT => above the 1000 euros

                if (invoice.Customer != null && invoice.Customer.ShippingAddress != null)
                {
                    // Endless aisle invoice' goods are delivered at a later point in time
                    // so by definition this is of type FT: we have a customer, address and delivery is not right now
                    invoiceType = InvoiceType.FT;
                }
                else if (invoiceTotalAmount > FaturaInvoiceLimit)
                {
                    // Invoices above 1000 euros are faturas
                    invoiceType = InvoiceType.FT;
                }
                else
                {
                    // Below 1000 euros so just a fatura simplificado
                    invoiceType = InvoiceType.FS;
                }
            }

            var hasReturns = invoice.Lines.Any(l => l.Quantity < 0);
            if (hasReturns)
            {
                invoiceType = InvoiceType.NC;
            }

            return invoiceType;
        }
    }
}