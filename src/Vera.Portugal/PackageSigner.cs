using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vera.Signing;

namespace Vera.Portugal
{
    public sealed class PackageSigner : IPackageSigner
    {
        private readonly RSA _rsa;

        public PackageSigner(RSA rsa)
        {
            _rsa = rsa;
        }

        public Task<PackageSignResult> Sign(Package package)
        {
            const string separator = ";";

            var invoiceDate = package.Timestamp.ToString("yyyy-MM-dd");
            var systemEntryDate = package.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss");
            var grossTotal = Math.Abs(package.Net).ToString("0.00", CultureInfo.InvariantCulture);
            var invoiceNo = package.Number;

            var signature = new StringBuilder()
                .Append(invoiceDate)
                .Append(separator)
                .Append(systemEntryDate)
                .Append(separator)
                .Append(invoiceNo)
                .Append(separator)
                .Append(grossTotal)
                .Append(separator)
                .Append(package.PreviousSignature)
                .ToString();

            var result = _rsa.SignData(
                Encoding.UTF8.GetBytes(signature),
                HashAlgorithmName.SHA1,
                RSASignaturePadding.Pkcs1
            );

            return Task.FromResult(new PackageSignResult(signature, result));
        }
    }
}