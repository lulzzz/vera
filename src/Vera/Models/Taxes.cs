namespace Vera.Models
{
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
}