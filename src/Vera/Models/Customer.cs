namespace Vera.Models
{
    public class Customer
    {
        public string SystemID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string RegistrationNumber { get; set; }
        public string TaxRegistrationNumber { get; set; }
        public Address Address { get; set; }
    }
}