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
        // TODO(kevin): do we want to prevent this? is it required to always know the channel?
        Other = 0,

        /// <summary>
        /// Payment with a debit card.
        /// </summary>
        Debit = 1,

        /// <summary>
        /// Payment with a credit card.
        /// </summary>
        Credit = 2,

        /// <summary>
        /// Payment was made with physical cash.
        /// </summary>
        Cash = 3,

        /// <summary>
        /// Change that was returned to the customer in physical cash.
        /// </summary>
        Change = 4,

        /// <summary>
        /// Payment was made with a voucher. E.g a giftcard.
        /// </summary>
        Voucher = 5,

        /// <summary>
        /// Payment was made online. E.g PayPal, iDeal, etc.
        /// </summary>
        Online = 6       
    }
}