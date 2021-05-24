using System;

namespace Vera.Models
{
    public class PeriodRegisterEntry
    {
        /// <summary>
        /// Amount of cash that's phyiscally in the register when it was opening
        /// </summary>
        public decimal OpeningAmount { get; set; }

        /// <summary>
        /// Amount of cash that's phyiscally in the register when it was closing
        /// </summary>
        public decimal ClosingAmount { get; set; }

        public Guid RegisterId { get; set; }
    }
}
