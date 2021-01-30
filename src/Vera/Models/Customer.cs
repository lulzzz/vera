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

        public Address BillingAddress { get; set; }
        public Address ShippingAddress { get; set; }

        public BankAccount BankAccount { get; set; }
    }
}