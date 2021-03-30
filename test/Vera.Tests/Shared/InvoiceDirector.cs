using System;
using System.Linq;
using System.Text;
using Vera.Models;

namespace Vera.Tests.Shared
{
    public class InvoiceDirector
    {
        private readonly InvoiceBuilder _builder;
        private readonly Guid _accountId;
        private readonly string _supplierSystemId;

        public InvoiceDirector(InvoiceBuilder builder, Guid accountId, string supplierSystemId)
        {
            _builder = builder;
            _accountId = accountId;
            _supplierSystemId = supplierSystemId;
        }

        public void ConstructAnonymousWithoutLines()
        {
            _builder
                .Reset()
                .WithAccount(_accountId)
                .WithTerminal("1.1")
                .WithEmployee()
                .WithSupplier(_supplierSystemId);
        }

        public void ConstructAnonymousWithSingleProductPaidWithCash()
        {
            const decimal unitPrice = 1.99m;
            
            var product = new Product
            {
                Code = "COCA",
                Description = "Coca cola",
                Type = ProductType.Goods
            };

            ConstructAnonymousWithSingleProductPaidWithCash(product);
        }

        public void ConstructAnonymousWithSingleProductPaidWithCash(Product product)
        {
            const decimal unitPrice = 1.99m;
            
            ConstructAnonymousWithoutLines();

            _builder
                .WithPayment(unitPrice, PaymentCategory.Cash)
                .WithProductLine(1, unitPrice, 1.21m, TaxesCategory.High, product);
        }

        // public Invoice CreateInvoice(Guid accountId, decimal unitPrice, int quantity, decimal taxRate, PaymentCategory paymentCategory)
        // {
        //     var product = new Product
        //     {
        //         Code = "COCA",
        //         Description = "Coca cola",
        //         Type = ProductType.Goods
        //     };
        //
        //     var amountInTax = unitPrice * quantity * taxRate;
        //
        //     var invoice = _builder
        //         .Reset()
        //         .WithTerminal("1.1")
        //         .WithEmployee()
        //         .WithSupplier()
        //         .WithPayment(amountInTax, paymentCategory)
        //         .WithProductLine(quantity, unitPrice, taxRate, TaxesCategory.High, product)
        //         .Build();
        //
        //     invoice.AccountId = accountId;
        //
        //     return invoice;
        // }
        //
        // public Invoice CreatePurchaseMultipleProducts(Guid accountId, decimal unitPrice, PaymentCategory paymentCategory, decimal[] taxRates, decimal? discount = null, decimal? lineDiscount = null)
        // {
        //     var amountInTax = taxRates.Length * unitPrice;
        //
        //     var multipleProductsBuilder = _builder
        //         .Reset()
        //         .WithTerminal("1.1")
        //         .WithEmployee()
        //         .WithSupplier()
        //         .WithPayment(amountInTax, paymentCategory);
        //
        //     foreach (var taxRate in taxRates)
        //     {
        //         var product = new Product
        //         {
        //             Code = "COCA",
        //             Description = "Coca cola",
        //             Type = ProductType.Goods
        //         };
        //         
        //         if (lineDiscount.HasValue)
        //         {
        //             // should work like this, please adapt totals calculation (order discount cannot be applied at the moment)
        //             var lineAmountInTax = unitPrice * taxRate;
        //             multipleProductsBuilder.WithProductLineSettlement(product, TaxesCategory.High, unitPrice, taxRate, -1 * lineAmountInTax * lineDiscount.Value);
        //         }
        //         else
        //         {
        //             multipleProductsBuilder.WithProductLine(1, unitPrice, taxRate, TaxesCategory.High, product);
        //         }
        //     }
        //
        //     if (discount.HasValue)
        //     {
        //         multipleProductsBuilder.WithSettlement(discount.Value);
        //     }
        //
        //     var invoice = multipleProductsBuilder.Build();
        //     invoice.AccountId = accountId;
        //
        //     return invoice;
        // }
        //
        // public Invoice CreatePurchaseWithSettlement(Guid accountId, decimal unitPrice, int quantity, decimal taxRate, PaymentCategory paymentCategory, decimal discountRate)
        // {
        //     var product = new Product
        //     {
        //         Code = "COCA",
        //         Description = "Coca cola",
        //         Type = ProductType.Goods
        //     };
        //     
        //     var amountInTax = unitPrice * quantity * taxRate;
        //     
        //     var invoice = _builder
        //         .Reset()
        //         .WithTerminal("1.1")
        //         .WithEmployee()
        //         .WithSupplier()
        //         .WithPayment(amountInTax, paymentCategory)
        //         .WithProductLineSettlement(product, TaxesCategory.High, unitPrice, taxRate, (-1) * discountRate * amountInTax)
        //         .WithSettlement(discountRate)
        //         .Build();
        //
        //     invoice.AccountId = accountId;
        //
        //     return invoice;
        // }
        //
        // public Invoice CreatePurchaseWithLineSettlement(Guid accountId, decimal unitPrice, int quantity, decimal taxRate, PaymentCategory paymentCategory, decimal discount)
        // {
        //     var product = new Product
        //     {
        //         Code = "COCA",
        //         Description = "Coca cola",
        //         Type = ProductType.Goods
        //     };
        //
        //     var product2 = new Product
        //     {
        //         Code = "Beer",
        //         Description = "Heineken",
        //         Type = ProductType.Goods
        //     };
        //
        //     var amountInTax = unitPrice * quantity * taxRate;
        //
        //     var invoice = _builder
        //         .Reset()
        //         .WithTerminal("1.1")
        //         .WithEmployee()
        //         .WithSupplier()
        //         .WithPayment(amountInTax, paymentCategory)
        //         .WithProductLine(quantity, unitPrice, taxRate, TaxesCategory.High, product)
        //         .WithProductLine(quantity, unitPrice, taxRate, TaxesCategory.High, product2)
        //         .WithSettlement(discount)
        //         .Build();
        //
        //     invoice.AccountId = accountId;
        //
        //     return invoice;
        // }
        //
        // public Invoice CreateReturnInvoice(Guid accountId, decimal unitPrice, int quantity, decimal taxRate, PaymentCategory paymentCategory, Invoice originalInvoice)
        // {
        //     var amountInTax = unitPrice * quantity * taxRate;
        //
        //     var invoice = _builder
        //         .Reset()
        //         .WithTerminal("1.1")
        //         .WithEmployee()
        //         .WithSupplier()
        //         .WithPayment(amountInTax, paymentCategory)
        //         .WithReturnLine(originalInvoice.Lines.First().Product, TaxesCategory.High, unitPrice, quantity, taxRate, originalInvoice)
        //         .Build();
        //
        //     invoice.AccountId = accountId;
        //
        //     return invoice;
        // }
    }
}