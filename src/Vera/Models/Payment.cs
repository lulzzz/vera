namespace Vera.Models
{
    public class Payment
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
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