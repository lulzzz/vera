using System.Collections.Generic;
using Vera.Models;

namespace Vera.Reports
{
    public class ReceiptReportContext
    {
        /// <summary>
        /// It contains all report data associated with a register 
        /// </summary>
        public RegisterReport RegisterReport { get; set; }

        // Static text to be printed up top
        public string Header { get; set; }

        /// <summary>
        /// All of the prints that were made for the invoice.
        /// </summary>
        public ICollection<PrintTrail> Prints { get; set; } = new List<PrintTrail>();

        /// <summary>
        /// Indicates that this is the first and original print.
        /// </summary>
        public bool Original { get; set; }

        public byte[] Signature { get; set; }
    }
}
