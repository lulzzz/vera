using Vera.Models;

namespace Vera.Signing
{
    /// <summary>
    /// Responsible for generating a short form of a signature. These short forms are displayed
    /// on documents and/or used in machine readable codes.
    /// </summary>
    public interface IShortFormSignatureTransformer
    {
        /// <summary>
        /// Transforms the given base64 version of the signature to its short form.
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
        string Transform(Signature signature);
    }
}