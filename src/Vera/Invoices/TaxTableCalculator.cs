using System;
using Vera.Models;

namespace Vera.Invoices
{
    public sealed class TaxTable
    {
        public sealed class Entry
        {
            /// <summary>
            /// Base on which the taxes were calculated. E.g the net amount.
            /// </summary>
            public decimal Base { get; set; }

            /// <summary>
            /// Amount of taxes that apply. Base plus this would equal the total including taxes (gross).
            /// </summary>
            public decimal Value { get; set; }
        }

        public Entry? High { get; set; }
        public Entry? Low { get; set; }
        public Entry? Zero { get; set; }
        public Entry? Exempt { get; set; }
        public Entry? Intermediate { get; set; }

        /// <summary>
        /// Sum of all <see cref="Entry.Value"/> added together to get the total amount of
        /// taxes in this table.
        /// </summary>
        public decimal Total
        {
            get
            {
                var total = 0m;

                if (High != null) total += High.Value;
                if (Low != null) total += Low.Value;
                if (Zero != null) total += Zero.Value;
                if (Exempt != null) total += Exempt.Value;
                if (Intermediate != null) total += Intermediate.Value;

                return total;
            }
        }
    }

    public class TaxTableCalculator
    {
        // TODO(kevin): do the rounding to 2 decimals here already?
        public TaxTable Calculate(Invoice invoice)
        {
            var table = new TaxTable();

            foreach (var line in invoice.Lines)
            {
                var entry = line.Taxes.Category switch
                {
                    TaxesCategory.High => table.High ??= new TaxTable.Entry(),
                    TaxesCategory.Low => table.Low ??= new TaxTable.Entry(),
                    TaxesCategory.Zero => table.Zero ??= new TaxTable.Entry(),
                    TaxesCategory.Exempt => table.Exempt ??= new TaxTable.Entry(),
                    TaxesCategory.Intermediate => table.Intermediate ??= new TaxTable.Entry(),
                    _ => throw new ArgumentOutOfRangeException(nameof(line.Taxes.Category), "Unknown tax category")
                };

                entry.Base += line.Net;
                entry.Value += line.Gross - line.Net;
            }

            return table;
        }
    }
}