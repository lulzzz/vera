using Vera.Invoices;

namespace Vera.Models
{
    public class Totals
    {
        /// <summary>
        /// Summary of all the taxes on the invoice.
        /// </summary>
        public TaxTable Taxes { get; set; } = new();

        /// <summary>
        /// Sum of the net of the lines on the invoice.
        /// <see cref="InvoiceLine.Net"/>
        /// </summary>
        public decimal Net { get; set; }

        /// <summary>
        /// Sum of the gross of the lines on the invoice.
        /// <see cref="InvoiceLine.Gross"/>
        /// </summary>
        public decimal Gross { get; set; }
    }
}