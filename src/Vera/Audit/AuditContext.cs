namespace Vera.Audit
{
    public class AuditContext<T>
    {
        public Account Account { get; set; }
        public string SoftwareName { get; set; }
        public string SoftwareVersion { get; set; }

        // Name of the certificate that was given by the authority
        public string CertificateName { get; set; }

        // Number of the certificate that was given by the authority
        public string CertificateNumber { get; set; }

        public T Configuration { get; set; }
    }
}