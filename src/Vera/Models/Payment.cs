namespace Vera.Models
{
    public class Payment
    {
        /// <summary>
        /// Reference to the payment service provider that performed the payment.
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// Identifier of the type of payment. E.g. Visa, Mastercard, iDeal, Cash, etc.
        /// </summary>
        public string Method { get; set; }

        public string Description { get; set; }

        public PaymentCategory Category { get; set; }

        public decimal Amount { get; set; }
    }

    public enum PaymentCategory
    {
        Other = 0,
        Debit = 1,
        Credit = 2,
        Cash = 3,
        Voucher = 4,
        Online = 5
    }
}