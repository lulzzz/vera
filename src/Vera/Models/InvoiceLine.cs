using System.Collections.Generic;

namespace Vera.Models
{
    public class InvoiceLine
    {
        public Product Product { get; set; }

        public string Description { get; set; }

        public InvoiceLineType Type { get; set; }

        public int Quantity { get; set; }

        public TaxInformation Tax { get; set; }

        public decimal Net { get; set; }
        public decimal Gross { get; set; }

        public ICollection<Settlement> Settlements { get; set; }
    }

    public class Product
    {
        public string ArticleCode { get; set; }
        public string Description { get; set; }
        public ProductGroup Group { get; set; }
    }

    public class TaxInformation
    {
        public string Code { get; set; }

        public decimal Rate { get; set; }
        public decimal Base { get; set; }
        public decimal Amount { get; set; }

        public string ExemptionReason { get; set; }
        public string ExemptionCode { get; set; }
    }

    public enum ProductGroup
    {
        Other = 1
    }

    public enum InvoiceLineType
    {
        Goods = 0,
        Services = 1
    }
}