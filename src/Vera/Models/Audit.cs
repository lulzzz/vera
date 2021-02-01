using System;

namespace Vera.Models
{
    /// <summary>
    /// Entity containing the fact that an audit export has been performed. It contains the
    /// criteria and any results to go with it, like for SAF-T the location of the .xml file.
    /// </summary>
    public class Audit
    {
        public Guid Id { get; init; }

        /// <summary>
        /// Date on which the audit was generated.
        /// </summary>
        public DateTime Date { get; init; }

        /// <summary>
        /// Account for which the audit was generated.
        /// </summary>
        public Guid AccountId { get; init; }

        /// <summary>
        /// Supplier for whom the audit was generated. Part of the criteria.
        /// </summary>
        public string SupplierSystemId { get; init; }

        /// <summary>
        /// Start date range of the audit. Part of the criteria.
        /// </summary>
        public DateTime Start { get; init; }

        /// <summary>
        /// End date range of the audit. Part of the criteria.
        /// </summary>
        public DateTime End { get; init; }

        /// <summary>
        /// Location of the resulting export.
        /// </summary>
        public string Location { get; set; }
    }
}