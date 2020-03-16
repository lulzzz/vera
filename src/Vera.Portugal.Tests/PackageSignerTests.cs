using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vera.Signing;
using Xunit;

namespace Vera.Portugal.Tests
{
    public class PackageSignerTests
    {
        [Fact]
        public async Task Should_build_signature_in_correct_format()
        {
            var package = new Package
            {
                Timestamp = new DateTime(1999, 10, 20, 13, 31, 22),
                Number = "t123/1",
                Net = -123.2323m,
                Gross =- 123.2323m,
                PreviousSignature = Encoding.UTF8.GetBytes("abcdefg")
            };

            var expectedSignature = "1999-10-20;1999-10-20T13:31:22;t123/1;123.23;" +
                                    Convert.ToBase64String(package.PreviousSignature);

            var signer = new PackageSigner(RSA.Create());
            var result = await signer.Sign(package);

            Assert.NotNull(result.Input);
            Assert.NotNull(result.Output);

            Assert.Equal(expectedSignature, result.Input);
            Assert.Equal(256, result.Output.Length);
        }
    }
}
