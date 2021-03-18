using System;
using System.Linq;
using Vera.Models;

namespace Vera.Tests.Shared
{
    public class InvoiceDirector
    {
        private readonly InvoiceBuilder _builder;

        public InvoiceDirector(InvoiceBuilder builder)
        {
            _builder = builder;
        }

        public Invoice CreateEmptyAnonymous(Guid accountId)
        {
            var invoice = _builder
                .Reset()
                .WithTerminal("1.1")
                .WithEmployee()
                .WithSupplier()
                .Build();

            invoice.AccountId = accountId;

            return invoice;
        }

        public Invoice CreateAnonymousSingleProductPaidWithCash(Guid accountId)
        {
            var product = new Product
            {
                Code = "COCA",
                Description = "Coca cola",
                Type = ProductType.Goods
            };

            const decimal unitPrice = 1.99m;

            var invoice = _builder
                .Reset()
                .WithTerminal("1.1")
                .WithEmployee()
                .WithSupplier()
                .WithPayment(unitPrice, PaymentCategory.Cash)
                .WithProductLine(product, TaxesCategory.High, unitPrice, 1, 1.21m)
                .Build();

            invoice.AccountId = accountId;

            return invoice;
        }

        public Invoice CreateAnonymousSingleProductPaidWithCash(Guid accountId, Product product)
        {
            const decimal unitPrice = 1.99m;

            var invoice = _builder
                .Reset()
                .WithTerminal("1.1")
                .WithEmployee()
                .WithSupplier()
                .WithPayment(unitPrice, PaymentCategory.Cash)
                .WithProductLine(product, TaxesCategory.High, unitPrice, 1, 1.21m)
                .Build();

            invoice.AccountId = accountId;

            return invoice;
        }

        public Invoice CreateInvoice(Guid accountId, decimal unitPrice, int quantity, decimal taxRate, PaymentCategory paymentCategory)
        {
            var product = new Product
            {
                Code = "COCA",
                Description = "Coca cola",
                Type = ProductType.Goods
            };

            var amountInTax = unitPrice * quantity * taxRate;

            var invoice = _builder
                .Reset()
                .WithTerminal("1.1")
                .WithEmployee()
                .WithSupplier()
                .WithPayment(amountInTax, paymentCategory)
                .WithProductLine(product, TaxesCategory.High, unitPrice, quantity, taxRate)
                .Build();

            invoice.AccountId = accountId;

            return invoice;
        }

        public Invoice CreatePurchaseMultipleProducts(Guid accountId, decimal unitPrice, PaymentCategory paymentCategory, decimal[] taxRates, decimal? discount = null, decimal? lineDiscount = null)
        {
            var amountInTax = taxRates.Length * unitPrice;

            var multipleProductsBuilder = _builder
                .Reset()
                .WithTerminal("1.1")
                .WithEmployee()
                .WithSupplier()
                .WithPayment(amountInTax, paymentCategory);

            foreach (var taxRate in taxRates)
            {
                var product = new Product
                {
                    Code = "COCA",
                    Description = "Coca cola",
                    Type = ProductType.Goods
                };
                if (lineDiscount.HasValue)
                {
                    // should work like this, please adapt totals calculation (order discount cannot be applied at the moment)
                    var lineAmountInTax = unitPrice * taxRate;
                    multipleProductsBuilder.WithProductLineSettlement(product, TaxesCategory.High, unitPrice, taxRate, (-1) * lineAmountInTax * lineDiscount.Value);
                }
                else
                {
                    multipleProductsBuilder.WithProductLine(product, TaxesCategory.High, unitPrice, 1, taxRate);
                }
            }

            if (discount.HasValue)
            {
                multipleProductsBuilder.WithSettlement(discount.Value);
            }

            var invoice = multipleProductsBuilder.Build();
            invoice.AccountId = accountId;

            return invoice;
        }

        public Invoice CreatePurchaseWithSettlement(Guid accountId, decimal unitPrice, int quantity, decimal taxRate, PaymentCategory paymentCategory, decimal discountRate)
        {
            var product = new Product
            {
                Code = "COCA",
                Description = "Coca cola",
                Type = ProductType.Goods
            };
            var amountInTax = unitPrice * quantity * taxRate;
            var invoice = _builder
                .Reset()
                .WithTerminal("1.1")
                .WithEmployee()
                .WithSupplier()
                .WithPayment(amountInTax, paymentCategory)
                .WithProductLineSettlement(product, TaxesCategory.High, unitPrice, taxRate, (-1) * discountRate * amountInTax)
                .WithSettlement(discountRate)
                .Build();

            invoice.AccountId = accountId;

            return invoice;
        }

        public Invoice CreatePurchaseWithLineSettlement(Guid accountId, decimal unitPrice, int quantity, decimal taxRate, PaymentCategory paymentCategory, decimal discount)
        {
            var product = new Product
            {
                Code = "COCA",
                Description = "Coca cola",
                Type = ProductType.Goods
            };

            var product2 = new Product
            {
                Code = "Beer",
                Description = "Heineken",
                Type = ProductType.Goods
            };

            var amountInTax = unitPrice * quantity * taxRate;

            var invoice = _builder
                .Reset()
                .WithTerminal("1.1")
                .WithEmployee()
                .WithSupplier()
                .WithPayment(amountInTax, paymentCategory)
                .WithProductLine(product, TaxesCategory.High, unitPrice, quantity, taxRate)
                .WithProductLine(product2, TaxesCategory.High, unitPrice, quantity, taxRate)
                .WithSettlement(discount)
                .Build();

            invoice.AccountId = accountId;

            return invoice;
        }

        public Invoice CreateReturnInvoice(Guid accountId, decimal unitPrice, int quantity, decimal taxRate, PaymentCategory paymentCategory, Invoice originalInvoice)
        {
            var amountInTax = unitPrice * quantity * taxRate;

            var invoice = _builder
                .Reset()
                .WithTerminal("1.1")
                .WithEmployee()
                .WithSupplier()
                .WithPayment(amountInTax, paymentCategory)
                .WithReturnLine(originalInvoice.Lines.First().Product, TaxesCategory.High, unitPrice, quantity, taxRate, originalInvoice)
                .Build();

            invoice.AccountId = accountId;

            return invoice;
        }
    }
}