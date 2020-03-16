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
    }
}