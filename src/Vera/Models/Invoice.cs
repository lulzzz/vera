namespace Vera.Models
{
    public sealed class Invoice
    {
        public string StoreNumber { get; set; }

        public int Sequence { get; set; }

        public bool Manual { get; set; }

        public string RawSignature { get; set; }
        public byte[] Signature { get; set; }
    }
}