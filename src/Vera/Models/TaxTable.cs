namespace Vera.Models
{
    public sealed class TaxTable
    {
        public Entry? High { get; set; }
        public Entry? Low { get; set; }
        public Entry? Zero { get; set; }
        public Entry? Exempt { get; set; }
        public Entry? Intermediate { get; set; }

        /// <summary>
        /// Sum of all <see cref="Entry.Value"/> added together to get the total amount of
        /// taxes in this table.
        /// </summary>
        public decimal Total =>
            High?.Value ?? 0 +
            Low?.Value ?? 0 +
            Zero?.Value ?? 0 +
            Exempt?.Value ?? 0 +
            Intermediate?.Value ?? 0;

        public sealed class Entry
        {
            public Entry(decimal rate)
            {
                Rate = rate;
            }

            /// <summary>
            /// Base on which the taxes were calculated. E.g the net amount.
            /// </summary>
            public decimal Base { get; set; }

            /// <summary>
            /// Amount of taxes that apply. Base plus this would equal the total including taxes (gross).
            /// </summary>
            public decimal Value { get; set; }

            public decimal Rate { get; }
        }
    }
}