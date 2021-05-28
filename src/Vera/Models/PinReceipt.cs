using System.Collections.Generic;

namespace Vera.Models
{
    public sealed class PinReceipt
    {
        /// <summary>
        /// Lines as received from the payment service provider.
        /// </summary>
        public IEnumerable<string> Lines { get; set; }
        
        public string SignatureMimeType { get; set; }
        
        /// <summary>
        /// Base64 encoded signature (if any).
        /// </summary>
        public byte[] SignatureData { get; set; }
    }
}