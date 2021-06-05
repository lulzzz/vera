using System;
using System.Collections.Generic;
using Vera.Models;

namespace Vera.Periods
{
    public class PeriodClosingContext
    {
        public Account Account { get; set; }

        /// <summary>
        /// The period that is being closed.
        /// </summary>
        public Period Period { get; set; }

        /// <summary>
        /// Employee that initiated the closing of the period.
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        /// Registers that are being closed during the period closing.
        /// </summary>
        public ICollection<RegisterEntry> Registers { get; init; } = new List<RegisterEntry>();

        public class RegisterEntry
        {
            public Guid Id { get; init; }
            public decimal ClosingAmount { get; init; }
        }
    }
}
