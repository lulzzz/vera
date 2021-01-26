using Vera.Models;

namespace Vera.Signing
{
    public interface IMachineReadableCodeGenerator
    {
        /// <summary>
        /// Generates the machine readable code for the given invoice. A machine readable code
        /// can for example be a QR code that's rendered on a receipt.
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns></returns>
        string Generate(Invoice invoice);
    }
}