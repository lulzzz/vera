using System;
using Vera.Signing;

namespace Vera.Portugal
{
    public class ShortFormSignatureTransformer : IShortFormSignatureTransformer
    {
        public string Transform(string signature)
        {
            // For details see: 2.2.2 of Order No. 8632/2014 of the 3 of July
            // file can be found in the docs folder

            if (string.IsNullOrEmpty(signature))
            {
                throw new NullReferenceException(nameof(signature));
            }

            if (signature.Length < 31)
            {
                throw new ArgumentOutOfRangeException(nameof(signature));
            }

            return string.Concat(
                signature[0],
                signature[10],
                signature[20],
                signature[30]
            );
        }
    }
}