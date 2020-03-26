using System.Collections.Generic;

namespace Vera.Models
{
    public class InvoiceLine
    {
        public int Quantity { get; set; }
        public string Description { get; set; }
        public decimal TaxRate { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountInTax => Amount * TaxRate;
        public ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();
        public Taxes Taxes { get; set; }
    }

    public class Taxes
    {
        public decimal Rate { get; set; }
        public string Code { get; set; }
    }
}