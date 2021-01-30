using Vera.Models;

namespace Vera.Audit
{
    public class AuditContext
    {
        public Account Account { get; set; }

        /// <summary>
        /// Name of the company that is behind the generation of the exports.
        /// </summary>
        public string SoftwareCompanyName { get; set; }

        /// <summary>
        /// Name of the software that does the certification.
        /// </summary>
        public string SoftwareName { get; set; }

        /// <summary>
        /// Version of the software. Note that each change to this value would require a new
        /// auditing trial.
        /// </summary>
        public string SoftwareVersion { get; set; }

        // Name of the certificate that was given by the authority
        public string CertificateName { get; set; }

        // Number of the certificate that was given by the authority
        public string CertificateNumber { get; set; }
    }
}