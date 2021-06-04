using System;

namespace Vera.Models
{
    public class Supplier
    {
        public Supplier()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public string SystemId { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Registration number of the business
        /// </summary>
        public string RegistrationNumber { get; set; }

        /// <summary>
        /// Registration number when reporting taxes
        /// </summary>
        public string TaxRegistrationNumber { get; set; }

        /// <summary>
        /// Address is the physical location of the supplier
        /// </summary>
        public Address Address { get; set; }

        /// <summary>
        /// The id of the account that can operate on this supplier
        /// </summary>
        public Guid AccountId { get; set; }
        
        /// <summary>
        /// The id of the supplier timezone (ISO standard)
        /// </summary>
        public string TimeZone { get; set; }
        /// <summary>
        /// Email Address of the supplier
        /// </summary>
        public string EmailAddress { get; set; }
    }
}
