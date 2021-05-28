using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vera.Configuration;

namespace Vera.Models
{
    /// <summary>
    /// Placeholder/shell for stores that reside within the same country. A good example
    /// would be to have an account "Norway", where all the stores that exist in the country
    /// Norway would exist below.
    /// </summary>
    public class Account
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Reference to the <see cref="Company"/> that this account belongs to.
        /// </summary>
        public Guid CompanyId { get; set; }

        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Registration number of the company. For example in the Netherlands this would
        /// be the KvK number (chamber of commerce).
        /// </summary>
        public string RegistrationNumber { get; set; }

        /// <summary>
        /// Tax registration number. For example in the Netherlands this would
        /// be the "BTW" number. It's the number given to you by the tax authorities.
        /// </summary>
        public string TaxRegistrationNumber { get; set; }

        /// <summary>
        /// Local address of the company.
        /// </summary>
        public Address Address { get; set; }

        /// <summary>
        /// Certification provider to use for this account.
        /// </summary>
        [Required]
        public string Certification { get; set; }

        /// <summary>
        /// ISO 4217 currency code that is used by the company.
        /// </summary>
        [Required]
        public string Currency { get; set; }

        /// <summary>
        /// Contact email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Contact phone number.
        /// </summary>
        public string Telephone { get; set; }

        /// <summary>
        /// Configuration data that is used for specific properties that are required
        /// for the configured <see cref="Certification"/>. E.g for Portugal a property like
        /// the "social capital" is required.
        /// </summary>
        public IDictionary<string, string> Configuration { get; set; }
        
        public ICollection<TaxRate> TaxRates { get; set; }
    }
}