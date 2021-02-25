using System.Collections.Generic;

namespace Vera.Models
{
    public class InvoiceLine
    {
        /// <summary>
        /// Quantity that was invoiced.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Description of the invoice line. Useful in case of a service rather than goods.
        /// </summary>
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
        /// Product that was invoiced in case of goods.
        /// </summary>
        public Product? Product { get; set; }

        public CreditReference? CreditReference { get; set; }

        /// <summary>
        /// Total amount including taxes.
        /// </summary>
        public decimal Gross { get; set; }
        
        /// <summary>
        /// Total amount excluding taxes. 
        /// </summary>
        public decimal Net { get; set; }

        /// <summary>
        /// Taxes that apply to the line.
        /// </summary>
        public Taxes Taxes { get; set; }

        /// <summary>
        /// Settlements specific to this line.
        /// </summary>
        public ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();
    }

    public class CreditReference
    {
        /// <summary>
        /// Reference to the original invoice by it's <see cref="Invoice.Number"/>.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Reason of crediting this line. E.g reason it's being returned.
        /// </summary>
        public string Reason { get; set; }
    }
}