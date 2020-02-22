using System;

namespace Vera.Models
{
    public sealed class Invoice
    {
        public Invoice(Invoice other)
        {
            StoreNumber = other.StoreNumber;
            Sequence = other.Sequence;
            Manual = other.Manual;
            RawSignature = other.RawSignature;
            Signature = other.Signature;
        }

        public Invoice() { }

        public string StoreNumber { get; set; }

        public int Sequence { get; set; }

        public bool Manual { get; set; }

        public string RawSignature { get; set; }
        public byte[] Signature { get; set; }
    }
}