using System;

namespace Vera.Models
{
    public class PeriodRegisterEntry
    {
        /// <summary>
        /// Amount of cash that's physically in the register when it was opening
        /// </summary>
        public decimal OpeningAmount { get; set; }

        /// <summary>
        /// Amount of cash that's physically in the register when it was closing
        /// </summary>
        public decimal ClosingAmount { get; set; }

        /// <summary>
        /// <see cref="Register.Id"/>
        /// </summary>
        public Guid RegisterId { get; set; }

        /// <summary>
        /// <see cref="Register.SystemId"/>
        /// </summary>
        public string RegisterSystemId { get; set; }
    }
}
