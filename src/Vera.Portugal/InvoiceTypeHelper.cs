using Vera.Models;

namespace Vera.Portugal
{
    public static class InvoiceTypeHelper
    {
        public static int DetermineType(Invoice invoice)
        {
            // TODO(kevin): fill this in

            // if total lines == total payments > receipt
            // if returns > credit
            // else return > default

            // // Invoice
            // invoice.BackendTypeID = (int)InvoiceType.FT;
            //
            // if (order.IsPaid)
            // {
            //     // Invoice receipt
            //     invoice.BackendTypeID = (int)InvoiceType.FR;
            // }
            //
            // if (order.HasReturns)
            // {
            //     // Credit note
            //     invoice.BackendTypeID = (int)InvoiceType.NC;
            // }

            return 0;
        }
    }
}