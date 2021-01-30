using System;

namespace Vera.Models
{
    public class Payment
    {
        /// <summary>
        /// Identifier of the payment in the external system.
        /// </summary>
        public string SystemId { get; set; }

        /// <summary>
        /// Date that the payment was performed.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Description of the payment.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Amount that was paid.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Category that the payment belongs to.
        /// </summary>
        public PaymentCategory Category { get; set; }
    }

    public enum PaymentCategory
    {
        Other = 0,
        Debit = 1,
        Credit = 2,
        Cash = 3,
        Change = 4,
        Voucher = 5,
        Online = 6       
    }
}