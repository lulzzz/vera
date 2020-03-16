namespace Vera.Models
{
    public class Billable
    {
        public string SystemID { get; set; }

        public string Name { get; set; }

        public Address Address { get; set; }

        public string RegistrationNumber { get; set; }
        public string TaxRegistrationNumber { get; set; }
    }
}