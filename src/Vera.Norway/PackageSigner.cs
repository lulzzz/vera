using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Signing;

namespace Vera.Norway
{
    public sealed class PackageSigner : IPackageSigner
    {
        private readonly RSA _rsa;
        private readonly int _privateKeyVersion;

        public PackageSigner(RSA rsa, int privateKeyVersion)
        {
            _rsa = rsa ?? throw new NullReferenceException(nameof(rsa));
            _privateKeyVersion = privateKeyVersion;
        }

        public Task<Signature> Sign(Package package)
        {
            const char separator = ';';

            const string firstSignature = "0";

            // Explicitly chose to use the names from the docs for the vars below
            var transDate = package.Timestamp.ToString("yyyy-MM-dd");
            var transTime = package.Timestamp.ToString("HH:mm:ss");
            var nr = package.Number;
            var transAmntIn = package.Gross.ToString("F", CultureInfo.InvariantCulture);
            var transAmntEx = package.Net.ToString("F", CultureInfo.InvariantCulture);

            var previousSignature = firstSignature;
            if (package.PreviousSignature != null)
            {
                // When there is no previous signature, signature should have the value "0"
                var tempSignature = Encoding.UTF8.GetString(package.PreviousSignature);
                previousSignature = !string.IsNullOrEmpty(tempSignature) ? tempSignature : previousSignature;
            }

            var sb = new StringBuilder();
            sb.Append(previousSignature).Append(separator);
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
