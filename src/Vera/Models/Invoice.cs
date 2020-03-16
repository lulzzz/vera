using System;
using System.Collections.Generic;

namespace Vera.Models
{
    // Invoice that is saved
    // Invoice model that is used for printing and auditing

    public class Invoice
    {
        public Invoice()
        {
        }

        public Invoice(Invoice other)
        {
            // TODO(kevin): map properties
        }

        public Guid AccountId { get; set; }

        public string SystemId { get; set; }

        public DateTime Date { get; set; }
        public string Number { get; set; }

        /// <summary>
        /// Indicates if the invoice was created manually.
        /// </summary>
        public bool Manual { get; set; }

        public int Sequence { get; set; }
        public string RawSignature { get; set; }
        public byte[] Signature { get; set; }

        public Billable Supplier { get; set; }
        public Billable Employee { get; set; }
        public Customer Customer { get; set; }

        /// <summary>
        /// Identifier of the cash register that created the invoice.
        /// </summary>
        public string TerminalId { get; set; }

        public int FiscalPeriod { get; set; }
        public int FiscalYear { get; set; }

        /// <summary>
        /// Identifiers of the orders from which this invoice is made up.
        /// </summary>
        public ICollection<string> OrderReferences { get; } = new List<string>();

        public string Remark { get; set; }

        public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Print> Prints { get; set; } = new List<Print>();
    }
}