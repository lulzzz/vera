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
        /// Total amount excluding taxes, but including any discounts. This is the sum of the net of the lines on the
        /// invoice.
        /// <see cref="InvoiceLine.Net"/>
        /// </summary>
        public decimal Net { get; set; }

        /// <summary>
        /// Total amount including taxes and discounts. This is the sum of the gross of the lines on the invoice.
        /// <see cref="InvoiceLine.Gross"/>
        /// </summary>
        public decimal Gross { get; set; }
    }
}