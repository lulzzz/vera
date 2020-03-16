using System.Collections.Generic;

namespace Vera.Models
{
    public class Totals
    {
        public ICollection<TaxTotal> Taxes { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountInTax { get; set; }
    }

    public class TaxTotal
    {
        public decimal Base { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }
}