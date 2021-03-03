using System;
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
            return _builder
                .Reset()
                .WithTerminal("1.1")
                .WithEmployee()
                .WithSupplier()
                .Build();
        }
        
        public Invoice CreateAnonymousSingleProductPaidWithCash(Guid accountId)
        {
            var product = new Product
            {
                Code = "COCA",
                Description = "Coca cola",
                Type = ProductType.Goods
            };

            const decimal amountInTax = 1.99m;
            
            var invoice = _builder
                .Reset()
                .WithTerminal("1.1")
                .WithEmployee()
                .WithSupplier()
                .WithPayment(amountInTax, PaymentCategory.Cash)
                .WithProductLine(product, TaxesCategory.High, amountInTax, 1.21m)
                .Build();

            invoice.AccountId = accountId;
            
            return invoice;
        }
    }
}