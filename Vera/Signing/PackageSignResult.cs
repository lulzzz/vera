namespace Vera.Signing
{
    public sealed class PackageSignResult
    {
        public PackageSignResult(string input, byte[] output)
        {
            Input = input;
            Output = output;
        }

        /// <summary>
        /// Input that was used to create the output.
        /// </summary>
        public string Input { get; }

        /// <summary>
        /// Output that was generated based on the input.
        /// </summary>
        public byte[] Output { get; }
    }
}