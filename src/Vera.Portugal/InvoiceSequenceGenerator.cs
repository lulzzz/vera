using System.Text;
using Vera.Models;

namespace Vera.Portugal
{
    public sealed class InvoiceSequenceGenerator : IInvoiceSequenceGenerator
    {
        public string Generate(Invoice invoice)
        {
            var sb = new StringBuilder(invoice.StoreNumber);

            // TODO(kevin): extract 'determine type' logic from number generator somehow and append it to the sequence

            if (invoice.Manual)
            {
                sb.Append("m");
            }

            return sb.ToString();
        }
    }
}