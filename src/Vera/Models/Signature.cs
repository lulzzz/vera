namespace Vera.Models
{
    public class Signature
    {
        public Signature() { }

        public Signature(string input, byte[] output)
        {
            Input = input;
            Output = output;
        }
        
        // /// <summary>
        // /// Input that was used to eventually generate the <see cref="Signature"/>.
        // /// </summary>
        public string Input { get; set; }

        // /// <summary>
        // /// Signature as bytes that is based on the implementation of the <see cref="Vera.Signing.IPackageSigner"/>.
        // /// </summary>
        public byte[] Output { get; set; }

        /// <summary>
        /// Optional. Contains the version of the certificate that was used to generate the signature.
        /// Required by some countries.
        /// </summary>
        public int? Version { get; set; }
    }
}