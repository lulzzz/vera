using System;
using System.Collections.Generic;
using Vera.Invoices;
using Vera.Models;
using Xunit;

namespace Vera.Tests
{
    // TODO(kevin): elaborate tests
    public class TaxTableCalculatorTests
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
                        Gross = 12.99m,
                        Net = 12.99m / 1.23m,
                        Taxes = new ()
                        {
                            Rate = 1.23m,
                            Category = TaxesCategory.High
                        }
                    }
                }
            };

            var calculator = new TaxTableCalculator();
            var table = calculator.Calculate(input);

            Assert.NotNull(table.High);
            Assert.Equal(10.56m, Math.Round(table.High.Base, 2));
            Assert.Equal(2.43m, Math.Round(table.High.Value, 2));

            Assert.Equal(2.43m, Math.Round(table.Total, 2));
        }
    }
}