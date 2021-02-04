using System.Collections.Generic;
using Vera.Models;

namespace Vera.Thermal
{
    public class ThermalReceiptContext
    {
        /// <summary>
        /// Account that the invoice belongs to.
        /// </summary>
        public Account Account { get; set; }

        /// <summary>
        /// Invoice being printed.
        /// </summary>
        public Invoice Invoice { get; set; }

        /// <summary>
        /// All of the prints that were made for the invoice.
        /// </summary>
        public ICollection<PrintTrail> Prints { get; set; } = new List<PrintTrail>();

        /// <summary>
        /// Indicates that this is the first and original print.
        /// </summary>
        public bool Original { get; set; }

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