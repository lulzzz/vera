using System.Collections.Generic;
using Vera.Documents.Nodes;
using Vera.Models;

namespace Vera.Documents
{
    public sealed class ThermalReceiptContext
    {
        public Account Account { get; set; }
        public Invoice Invoice { get; set; }
        public Totals Totals { get; set; }

        public string SoftwareVersion { get; set; }
        public string CertificateName { get; set; }
        public string CertificateNumber { get; set; }

        public byte[] HeaderImage { get; set; }
        public string HeaderImageMimeType { get; set; }
        
        // Text to be printed up top
        public IEnumerable<string> Header { get; set; }

        // Text to be printed down below
        public IEnumerable<string> Footer { get; set; }
    }

    public interface IThermalReceiptGenerator
    {
        IThermalNode Generate(ThermalReceiptContext context);
    }
}