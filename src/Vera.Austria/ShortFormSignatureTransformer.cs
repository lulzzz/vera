using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Text;
using Vera.Models;
using Vera.Signing;

namespace Vera.Austria
{
    public class ShortFormSignatureTransformer : IShortFormSignatureTransformer
    {
        public string Transform(Signature signature)
        {
            var value = Encoding.UTF8.GetString(signature.Output);

            var parts = value.Split('.');

            if (parts.Length != 3)
            {
                // Expect JWS to be 3 pieces: header, payload and checksum
                throw new InvalidOperationException("Expected signature to be a JWS");
            }

            return new StringBuilder()
              .Append(ExtractPayload(parts[1]))
              .Append('_')
              .Append(ExtractSignature(parts[2]))
              .ToString();
        }

        private static string ExtractPayload(string payload)
        {
            // Payload contains the signature of the receipt
            // e.g. _R1-AT0_DEMO-CASH-BOX524_366588_2015-12-17T11:23:44_0,00_0,00_0,00_26,05_0,00_m8LGyyY4UAA=_20f2ed172daa09e5_5HjRCx+XIz4=
            // in the template of the receipt this ^ is rendered as a QR code
            return Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(payload));
        }

        /// <summary>
        /// Extracts the signature from the JWT (the last part) based on the specs in the Detailfragen 4.1.
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
        private static string ExtractSignature(string signature)
        {
            return Convert.ToBase64String(WebEncoders.Base64UrlDecode(signature));
        }
    }
}
