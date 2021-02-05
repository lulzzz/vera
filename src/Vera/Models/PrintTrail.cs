using System;

namespace Vera.Models
{
    // TODO: generic audit trail model?
    // TODO: chain these?
    public class PrintTrail
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Invoice for which this trail was created.
        /// </summary>
        public Guid InvoiceId { get; set; }

        /// <summary>
        /// Date that the trail was created.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Indicates if the print was executed successfully according to the client.
        /// </summary>
        public bool Success { get; set; }
    }
}