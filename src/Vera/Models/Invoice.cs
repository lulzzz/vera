using System;
using System.Collections.Generic;

namespace Vera.Models
{
    public class Invoice
    {
        public Invoice()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the <see cref="Account"/> for which the invoice was created.
        /// </summary>
        public Guid AccountId { get; set; }

        /// <summary>
        /// SystemId is the identifier of the invoice in the external system.
        /// </summary>
        public string SystemId { get; set; }

        /// <summary>
        /// Date that his invoice was created.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Unique number of this invoice.
        /// </summary>
        public string Number { get; set; }

        public string ReturnedInvoiceNumber { get; set; }

        /// <summary>
        /// Indicates if the invoice was created manually.
        /// </summary>
        public bool Manual { get; set; }

        /// <summary>
        /// Unique sequence number of the invoice in the chain that it belongs to.
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// Signature details of the invoice.
        /// </summary>
        public Signature Signature { get; set; } = new();

        /// <summary>
        /// Supplier is the seller of the goods/services on the invoice.
        /// </summary>
        public Billable Supplier { get; set; }

        /// <summary>
        /// Employee that was responsible for creating the invoice.
        /// </summary>
        public Employee Employee { get; set; }

        /// <summary>
        /// Customer that purchased the order. Optional because it could also be bought
        /// anonymously.
        /// </summary>
        public Customer? Customer { get; set; }

        /// <summary>
        /// Address that the invoice goods/services should be delivered to.
        /// </summary>
        public Address? ShipTo { get; set; }

        /// <summary>
        /// Identifier of the cash register that created the invoice.
        /// </summary>
        public string TerminalId { get; set; }

        public int FiscalPeriod { get; set; }
        public int FiscalYear { get; set; }

        /// <summary>
        /// Generic free-for-all remark field. Generally used for printing on the receipt.
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// Summary of the totals on the invoice.
        /// </summary>
        public Totals Totals { get; set; } = new();

        /// <summary>
        /// Identifiers of the orders from which this invoice is made up.
        /// </summary>
        public ICollection<string> OrderReferences { get; set; } = new List<string>();

        /// <summary>
        /// Lines that make up this invoice.
        /// </summary>
        public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();

        /// <summary>
        /// Any settlements (discounts) that apply to the invoice.
        /// </summary>
        public ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

        /// <summary>
        /// Payments that were made to pay the invoice.
        /// </summary>
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    public class Signature
    {
        // /// <summary>
        // /// Input that was used to eventually generate the <see cref="Signature"/>.
        // /// </summary>
        public string Input { get; set; }

        // /// <summary>
        // /// Signature as bytes that is based on the implementation of the <see cref="Vera.Signing.IPackageSigner"/>.
        // /// </summary>
        public byte[] Output { get; set; }

        /// <summary>
        /// Optional. Contains the version of the certificate that was used to generate the signature.
        /// Required by some countries.
        /// </summary>
        public int? Version { get; set; }
    }
}