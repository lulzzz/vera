using System;
using System.Collections.Generic;
using Vera.Configuration;

namespace Vera.Models
{
    // "Rituals Norway"
    public class Account
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }

        public string Name { get; set; }
        public string RegistrationNumber { get; set; }
        public Address Address { get; set; }
        
        public string Certification { get; set; }

        public IDictionary<string, string> Configuration { get; set; }
        public string Currency { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string TaxRegistrationNumber { get; set; }
    }
}