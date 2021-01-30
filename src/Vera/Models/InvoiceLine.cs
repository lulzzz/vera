using System.Collections.Generic;

namespace Vera.Models
{
    public class InvoiceLine
    {
        public int Quantity { get; set; }

        public string Description { get; set; }
        
        /// <summary>
        /// Price per unit of measure.
        /// </summary>
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// Unit of measure of the product/service.
        /// </summary>
        public string UnitOfMeasure { get; set; }
        
        /// <summary>
        /// Total amount including taxes.
        /// </summary>
        public decimal Gross { get; set; }
        
        /// <summary>
        /// Total amount excluding taxes. 
        /// </summary>
        public decimal Net { get; set; }
        
        public ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();
        public Taxes Taxes { get; set; }
        public Product Product { get; set; }
    }

    public class Taxes
    {
        /// <summary>
        /// Percentage of the tax as a rate, so 1.21 for 21%.
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// Tax code that applies to this tax.
        /// </summary>
        public string Code { get; set; }

        public TaxesCategory Category { get; set; }

        /// <summary>
        /// Code for the tax exemption.
        /// </summary>
        public string ExemptionCode { get; set; }

        /// <summary>
        /// Reason for the tax exemption.
        /// </summary>
        public string ExemptionReason { get; set; }
    }

    public enum TaxesCategory
    {
        High,
        Low,
        Zero,
        Exempt,
        Intermediate
    }
    
    public class Product
    {
        public string SystemId { get; set; }
        public ProductTypes Type { get; set; }
        public string Code { get; set; }
        public string Barcode { get; set; }
        public string Description { get; set; }
    }

    public enum ProductTypes
    {
        Service = 1,
        Goods = 2
    }
}