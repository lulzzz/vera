using System;
using System.Collections.Generic;

namespace Vera.Models
{
    public class Period
    {
        public Period()
        {
            Id = Guid.NewGuid();
            Registers = new List<PeriodRegisterEntry>();
        }

        public Guid Id { get; set; }

        public Guid SupplierId { get; init; }

        /// <summary>
        /// The opening time of the period, the date time when the period was created
        /// </summary>
        public DateTime Opening { get; init; }

        /// <summary>
        /// The closing time of the period, the date time when the period was closed
        /// </summary>
        public DateTime? Closing { get; set; }

        public bool IsClosed => Closing.HasValue;

        /// <summary>
        /// A collection of registers in a certain period
        /// </summary>
        public ICollection<PeriodRegisterEntry> Registers { get; }
    }
}
