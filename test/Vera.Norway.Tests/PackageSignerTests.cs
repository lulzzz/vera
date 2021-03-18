using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vera.Invoices;
using Vera.Models;
using Vera.Signing;
using Xunit;

namespace Vera.Norway.Tests
{
    public class PackageSignerTests
    {
        [Fact]
        public async Task Should_generate_correct_signature()
        {
            var package = new Package
            {
                Timestamp = new DateTime(1999, 10, 20, 13, 31, 22),
                Number = "t123/1",
                Net = -100m,
                Gross = -123.2323m,
                PreviousSignature = Encoding.UTF8.GetBytes("abcdefg")
            };

            const string expectedSignature = "abcdefg;1999-10-20;13:31:22;t123/1;-123.23;-100.00;";

            var signer = new PackageSigner(RSA.Create(), 1);
            var result = await signer.Sign(package);

            Assert.Equal(expectedSignature, result.Input);
            Assert.Equal(256, result.Output.Length);
            Assert.Equal(1, result.Version);
        }

        [Fact]
        public async Task Should_generate_correct_first_signature()
        {
            var invoice = new Invoice
            {
                Date = new DateTime(1999, 10, 20, 13, 31, 22),
                Number = "1001",
                Lines = new List<InvoiceLine>
                {
                  new InvoiceLine
                  {
                    Quantity = 1,
                    UnitPrice = 100 / 1.25m,
                    Taxes = new Taxes { Rate = 1.25m },
                    Gross = 100,
                    Net = 100 / 1.25m
                  },
                  new InvoiceLine
                  {
                    Quantity = 1,
                    UnitPrice = 5 / 1.13m,
                    Taxes = new Taxes { Rate = 1.13m },
                    Gross = 5m,
                    Net = 5 / 1.13m
                  },
                }
            };
            invoice.Totals = new InvoiceTotalsCalculator().Calculate(invoice);

            const string expectedSignature = "0;1999-10-20;13:31:22;1001;105.00;84.42;";

            Signature prevSignature = null;
            var package2 = new Package(invoice, prevSignature);
            var signer2 = new PackageSigner(RSA.Create(), 1);
            var result2 = await signer2.Sign(package2);

            Assert.Equal(expectedSignature, result2.Input);
            Assert.Equal(256, result2.Output.Length);
            Assert.Equal(1, result2.Version);
        }
    }
}