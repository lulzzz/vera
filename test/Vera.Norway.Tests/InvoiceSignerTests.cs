using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vera.Invoices;
using Vera.Models;
using Xunit;

namespace Vera.Norway.Tests
{
    public class InvoiceSignerTests
    {
        [Fact]
        public async Task Should_generate_correct_signature()
        {
            var invoice = new Invoice
            {
                Date = new DateTime(1999, 10, 20, 13, 31, 22),
                Number = "t123/1",
                Totals = new Totals
                {
                    Net = -100m,
                    Gross = -123.2323m,
                }
            };
            var previousSignature = new Signature { Output = Encoding.UTF8.GetBytes("abcdefg") };

            const string expectedSignature = "abcdefg;1999-10-20;13:31:22;t123/1;-123.23;-100.00;";

            var signer = new InvoiceSigner(RSA.Create(), 1);
            var result = await signer.Sign(invoice, previousSignature);

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

            var signer2 = new InvoiceSigner(RSA.Create(), 1);
            var result2 = await signer2.Sign(invoice, null);

            Assert.Equal(expectedSignature, result2.Input);
            Assert.Equal(256, result2.Output.Length);
            Assert.Equal(1, result2.Version);
        }
    }
}