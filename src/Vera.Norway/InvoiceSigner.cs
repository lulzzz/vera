using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Signing;

namespace Vera.Norway
{
    public sealed class InvoiceSigner : IInvoiceSigner
    {
        private readonly RSA _rsa;
        private readonly int _privateKeyVersion;

        public InvoiceSigner(RSA rsa, int privateKeyVersion)
        {
            _rsa = rsa ?? throw new NullReferenceException(nameof(rsa));
            _privateKeyVersion = privateKeyVersion;
        }

        public Task<Signature> Sign(Invoice invoice, Signature previousSignature)
        {
            const char separator = ';';

            const string firstSignature = "0";

            // Explicitly chose to use the names from the docs for the vars below
            var transDate = invoice.Date.ToString("yyyy-MM-dd");
            var transTime = invoice.Date.ToString("HH:mm:ss");
            var nr = invoice.Number;
            var transAmntIn = invoice.Totals.Gross.ToString("F", CultureInfo.InvariantCulture);
            var transAmntEx = invoice.Totals.Net.ToString("F", CultureInfo.InvariantCulture);

            var signatureOutput = firstSignature;
            if (previousSignature != null)
            {
                // When there is no previous signature, signature should have the value "0"
                var tempSignature = Encoding.UTF8.GetString(previousSignature.Output);
                signatureOutput = !string.IsNullOrEmpty(tempSignature) ? tempSignature : signatureOutput;
            }

            var sb = new StringBuilder();
            sb.Append(signatureOutput).Append(separator);
            sb.Append(transDate).Append(separator);
            sb.Append(transTime).Append(separator);
            sb.Append(nr).Append(separator);
            sb.Append(transAmntIn).Append(separator);
            sb.Append(transAmntEx).Append(separator);

            var signature = sb.ToString();

            var result = _rsa.SignData(
                Encoding.UTF8.GetBytes(signature),
                HashAlgorithmName.SHA1,
                RSASignaturePadding.Pkcs1
            );

            return Task.FromResult(new Signature
            {
                Input = signature,
                Output = result,
                Version = _privateKeyVersion
            });
        }
    }
}
