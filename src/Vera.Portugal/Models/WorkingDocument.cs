using System;
using System.Collections.Generic;
using Vera.Models;

namespace Vera.Portugal.Models
{
    public class WorkingDocument
    {
        public WorkingDocument()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public string Number { get; set; }

        public int Sequence { get; set; }

        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Signature details of the invoice.
        /// </summary>
        public Signature Signature { get; set; } = new();

        /// <summary>
        /// Identifier of the <see cref="Invoice"/> for which the document was created.
        /// </summary>
        public Guid InvoiceId { get; set; }

        /// <summary>
        /// The period when this invoice was issued
        /// <seealso cref="Period"/>
        /// </summary>
        public Guid PeriodId { get; set; }

        /// <summary>
        /// System id of the <see cref="Supplier"/>.
        /// </summary>
        public string SupplierSystemId { get; set; }

        /// <summary>
        /// Lines that make up this invoice.
        /// </summary>
        public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    }
}
