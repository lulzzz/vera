namespace Vera.Models
{
    public class Customer
    {
        public string SystemId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string RegistrationNumber { get; set; }

        public string CompanyName { get; set; }
        public string TaxRegistrationNumber { get; set; }

        /// <summary>
        /// Address that the invoice is assigned to.
        /// </summary>
        public Address BillingAddress { get; set; }
        
        /// <summary>
        /// Address that the goods/services should be delivered to.
        /// </summary>
        public Address ShippingAddress { get; set; }

        public BankAccount BankAccount { get; set; }
    }
}