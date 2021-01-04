using System.Collections.Generic;
using Vera.Audit;
using Vera.Documents.Nodes;
using Vera.Models;

namespace Vera.Thermal
{
    public class ThermalReceiptContext : AuditContext
    {
        public Invoice Invoice { get; set; }
        public Totals Totals { get; set; }

        // Image to print on top of the receipt
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