using System;

namespace Vera.Models
{
    public class Period
    {
        public Period()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        /// <summary>
        /// The opening time of the period, the date time when the period was created
        /// </summary>
        public DateTime Opening { get; set; }

        /// <summary>
        /// The closing time of the period, the date time when the period was closed
        /// </summary>
        public DateTime Closing { get; set; }
        public Supplier Supplier { get; set; }

        public bool IsClosed => Closing != DateTime.MinValue;
    }
}
