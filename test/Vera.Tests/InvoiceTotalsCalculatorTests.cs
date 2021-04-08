using System;
using System.Collections.Generic;
using Vera.Extensions;
using Vera.Invoices;
using Vera.Models;
using Xunit;

namespace Vera.Tests
{
    // TODO(kevin): elaborate tests
    public class InvoiceTotalsCalculatorTests
    {
        [Fact]
        public void Should_correctly_calculate_amounts()
        {
            var input = new Invoice
            {
                Lines = new List<InvoiceLine>
                {
                    new()
                    {
                        Quantity = 1,
                        UnitPrice = 12.99m / 1.23m,
                        Taxes = new Taxes()
                        {
                            Rate = 1.23m,
                            Category = TaxesCategory.High
                        }
                    }
                }
            };

            var calculator = new InvoiceTotalsCalculator();
            var totals = calculator.Calculate(input);
            var table = totals.Taxes;
            var net = (12.99m / 1.23m).Round(2);

            Assert.Equal(12.99m, totals.Gross);
            Assert.Equal(net, totals.Net);

            Assert.NotNull(table.High);
            Assert.Equal(1.23m, table.High.Rate);
            Assert.Equal(10.56m, Math.Round(table.High.Base, 2));
            Assert.Equal(2.43m, Math.Round(table.High.Value, 2));
            Assert.Equal(2.43m, Math.Round(table.Total, 2));
        }
    }
}