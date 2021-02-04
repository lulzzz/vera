using System.Collections.Generic;
using Vera.Models;
using Vera.Portugal.Models;
using Xunit;

namespace Vera.Portugal.Tests
{
    public class InvoiceTypeHelperTests
    {
        [Fact]
        public void Should_be_FR_for_anonymous_invoice()
        {
            var input = new Invoice
            {
                // Below 1000 euros, no customer is attached and not a delivery
            };

            var output = InvoiceTypeHelper.DetermineType(input);

            Assert.Equal(InvoiceType.FR, output);
        }

        [Fact]
        public void Should_be_FT_for_delivery_invoice()
        {
            var input = new Invoice
            {
                Customer = new(),
                ShipTo =  new()
            };

            var output = InvoiceTypeHelper.DetermineType(input);

            Assert.Equal(InvoiceType.FT, output);
        }

        [Fact]
        public void Should_be_FT_for_invoice_above_1000_euros()
        {
            var input = new Invoice
            {
                Lines = new List<InvoiceLine>
                {
                    new()
                    {
                        Gross = 1001m
                    }
                }
            };

            var output = InvoiceTypeHelper.DetermineType(input);

            Assert.Equal(InvoiceType.FT, output);
        }

        [Fact]
        public void Should_be_FS_for_invoice_below_1000_euros_with_customer()
        {
            var input = new Invoice
            {
                Customer = new()
            };

            var output = InvoiceTypeHelper.DetermineType(input);

            Assert.Equal(InvoiceType.FS, output);
        }
    }
}