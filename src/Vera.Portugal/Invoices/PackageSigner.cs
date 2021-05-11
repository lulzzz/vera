using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Signing;

namespace Vera.Portugal.Invoices
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

            var date = package.Timestamp.ToString("yyyy-MM-dd");
            var systemEntryDate = package.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss");
            var grossTotal = Math.Abs(package.Gross).ToString("0.00", CultureInfo.InvariantCulture);
            var number = package.Number;

            var signatureBuilder = new StringBuilder()
                .Append(date)
                .Append(separator)
                .Append(systemEntryDate)
                .Append(separator)
                .Append(number)
                .Append(separator)
                .Append(grossTotal)
                .Append(separator);

            if (package.PreviousSignature != null)
            {
                signatureBuilder.Append(Convert.ToBase64String(package.PreviousSignature));
            }

            var signature = signatureBuilder.ToString();

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