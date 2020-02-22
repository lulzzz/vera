using System;

namespace Vera.Models
{
    public class Invoice
    {
        public Invoice(Invoice other)
        {
            StoreNumber = other.StoreNumber;
            Sequence = other.Sequence;
            Manual = other.Manual;
            RawSignature = other.RawSignature;
            Signature = other.Signature;
            FiscalPeriod = other.FiscalPeriod;
            FiscalYear = other.FiscalYear;
        }

        public Invoice() { }

        public string StoreNumber { get; set; }

        public int Sequence { get; set; }

        public bool Manual { get; set; }
        
        public int FiscalPeriod { get; set; }
        public int FiscalYear { get; set; }

        public string RawSignature { get; set; }
        public byte[] Signature { get; set; }
    }
}