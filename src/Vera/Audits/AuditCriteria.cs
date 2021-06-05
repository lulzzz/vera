using System;
using Vera.Models;

namespace Vera.Audits
{
    public sealed class AuditCriteria
    {
        /// <summary>
        /// Id of the account to generate the audit for.
        /// </summary>
        /// <see cref="Account.Id"/>
        public Guid AccountId { get; set; }

        /// <summary>
        /// SystemId of the supplier of the invoice(s).
        /// </summary>
        /// <see cref="Invoice.Supplier"/>
        public Guid SupplierId { get; set; }

        public Guid? RegisterId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
