using System.Collections.Generic;
using Vera.Models;

namespace Vera.Audits
{
    public class AuditContext
    {
        public Account Account { get; set; }

        /// <summary>
        /// Name of the company that is behind the generation of the exports.
        /// </summary>
        public string SoftwareCompanyName { get; set; } = "New Black";

        /// <summary>
        /// Name of the software that does the certification.
        /// </summary>
        public string SoftwareName { get; set; } = "Vera";

        /// <summary>
        /// Version of the software. Note that each change to this value would require a new
        /// auditing trial.
        /// </summary>
        public string SoftwareVersion { get; set; } = "0.1.0";

        public ICollection<Invoice> Invoices { get; set; }
    }
}