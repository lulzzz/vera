using System.Collections.Generic;
using Vera.Models;

namespace Vera.Thermal
{
    public class ThermalReceiptContext
    {
        public Account Account { get; set; }
        public Invoice Invoice { get; set; }

        // Image to print on top of the receipt
        public byte[] HeaderImage { get; set; }
        public string HeaderImageMimeType { get; set; }

        // Text to be printed up top
        public IEnumerable<string> Header { get; set; }

        // Text to be printed down below
        public IEnumerable<string> Footer { get; set; }

        /// <summary>
        /// Version of the software to generated the receipt. Some certifications require
        /// this to be printed on the receipt.
        /// </summary>
        public string SoftwareVersion { get; set; }
    }
}