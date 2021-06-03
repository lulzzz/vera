using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Portugal.Invoices;
using Xunit;

namespace Vera.Portugal.Tests
{
    public class InvoiceSignerTests
    {
        [Fact]
        public async Task Should_build_signature_in_correct_format()
        {
            var invoice = new Invoice
            {
                Date = new DateTime(1999, 10, 20, 13, 31, 22),
                Number = "t123/1",
                Totals = new Totals
                {
                    Net = -123.2323m,
                    Gross = -123.2323m,
                }
            };
            var previousSignature = new Signature { Output = Encoding.UTF8.GetBytes("abcdefg") };

            var expectedSignature = "1999-10-20;1999-10-20T13:31:22;t123/1;123.23;" +
                                    Convert.ToBase64String(previousSignature.Output);

            var signer = new InvoiceSigner(RSA.Create(), 1);
            var result = await signer.Sign(invoice, previousSignature);

            Assert.Equal(expectedSignature, result.Input);
            Assert.Equal(256, result.Output.Length);
            Assert.Equal(1, result.Version);
        }
    }
}
