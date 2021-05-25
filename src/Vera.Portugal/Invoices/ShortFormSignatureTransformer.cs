using System;
using Vera.Models;
using Vera.Signing;

namespace Vera.Portugal.Invoices
{
    public class ShortFormSignatureTransformer : IShortFormSignatureTransformer
    {
        public string Transform(Signature signature)
        {
            // For details see: 2.2.2 of Order No. 8632/2014 of the 3 of July
            // file can be found in the docs folder

            if (signature?.Output == null)
            {
                throw new NullReferenceException(nameof(signature));
            }
            
            var value = Convert.ToBase64String(signature.Output);

            if (value.Length < 31)
            {
                throw new ArgumentOutOfRangeException(nameof(signature));
            }

            return string.Concat(
                value[0],
                value[10],
                value[20],
                value[30]
            );
        }
    }
}