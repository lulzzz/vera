using System;

namespace Vera.Signing
{
    public sealed class Package
    {
        public DateTime Date { get; set; }

        public string Number { get; set; }

        public decimal Net { get; set; }
        public decimal Gross { get; set; }

        public string PreviousSignature { get; set; }
    }
}