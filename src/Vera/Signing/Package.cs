using System;
using System.Linq;
using Vera.Models;

namespace Vera.Signing
{
    public sealed class Package
    {
        public Package() { }

        public Package(Invoice invoice, Invoice previous)
        {
            Timestamp = invoice.Date;
            Number = invoice.Number;
            Net = invoice.Lines.Sum(l => l.Amount);
            Gross = invoice.Lines.Sum(l => l.AmountInTax);
            PreviousSignature = previous?.Signature;
        }

        /// <summary>
        /// Date and time that the package was created.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Number of the package (e.g. the number of the invoice).
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Total amount of the package excluding taxes.
        /// </summary>
        public decimal Net { get; set; }

        /// <summary>
        /// Total amount of the package including taxes.
        /// </summary>
        public decimal Gross { get; set; }

        /// <summary>
        /// Signature of the previous package (may be null if this is the first package).
        /// </summary>
        public byte[] PreviousSignature { get; set; }
    }
}