using System.Linq;
using Vera.Models;
using Vera.Portugal.Models;

namespace Vera.Portugal
{
    public static class InvoiceTypeHelper
    {
        public static InvoiceType DetermineType(Invoice invoice)
        {
            const decimal faturaInvoiceLimit = 1000m;

            // Ignore warning - want this to default hard to be explicit
            var invoiceType = InvoiceType.FT;

            var invoiceTotalAmount = invoice.Lines.Sum(l => l.Gross);

            // TODO(kevin): check what the definition is of an anonymous invoice
            if (invoiceTotalAmount < faturaInvoiceLimit && invoice.Customer == null)
            {
                // TODO(kevin): should check if amount > 1000 euros, then a customer is required
                // Invoice receipt because it's an anonymous order
                invoiceType = InvoiceType.FR;
            }
            else
            {
                // Customer is available, so we have 2 options
                // FS (fatura simplificado) or FT (invoice)
                // FS => NIF available, below 1000 euros and in-store transaction
                // FT => above the 1000 euros

                if (invoice.Customer != null && invoice.ShipTo != null)
                {
                    // Endless aisle order, good are delivered at a later point in time
                    // so by definition this is of type FT: we have a customer, address and delivery is not right now
                    invoiceType = InvoiceType.FT;
                }
                else if (invoiceTotalAmount > faturaInvoiceLimit)
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