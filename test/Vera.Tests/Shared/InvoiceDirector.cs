using System;
using System.Collections.Generic;
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
        
        public InvoiceDirector(InvoiceBuilder builder, string accountId, string supplierSystemId)
        {
            _builder = builder;
            _accountId = Guid.Parse(accountId);
            _supplierSystemId = supplierSystemId;
        }

        public void ConstructAnonymousWithoutLines()
        {
            _builder
                .Reset()
                .WithAccount(_accountId)
                .WithRegister("1.1")
                .WithEmployee()
                .WithSupplier(_supplierSystemId)
                .WithSignature(new Signature
                {
                    Input = "test",
                    Output = Encoding.ASCII.GetBytes("test"),
                    Version = 1
                });
        }

        public void ConstructAnonymousWithSingleProductPaidWithCash()
        {
            ConstructAnonymousWithSingleProductPaidWithCash(ProductFactory.CreateCocaCola());
        }

        public void ConstructAnonymousWithSingleProductPaidWithCash(Product product)
        {
            const decimal unitPrice = 1.99m;
            const decimal taxRate = 1.21m;
            
            ConstructAnonymousWithoutLines();

            _builder
                .WithPayment(unitPrice * taxRate, PaymentCategory.Cash)
                .WithProductLine(1, unitPrice, taxRate, TaxesCategory.High, product);
        }
        
        public void ConstructAnonymousWithSingleProductPaidWithCategory(
            decimal gross, 
            int quantity, 
            decimal taxRate, 
            PaymentCategory paymentCategory
        )
        {
            ConstructAnonymousWithoutLines();

            var product = ProductFactory.CreateCocaCola();
            
            _builder
                .WithPayment(gross, paymentCategory)
                .WithProductLine(quantity, gross / taxRate, taxRate, TaxesCategory.High, product);
        }
        
        public void ConstructWithTaxRates(
            decimal grossPerLine, 
            PaymentCategory paymentCategory, 
            IDictionary<TaxesCategory, decimal> rates
        )
        {
            ConstructAnonymousWithoutLines();

            _builder.WithPayment(grossPerLine * rates.Count, paymentCategory);
        
            foreach (var rate in rates)
            {
                var product = ProductFactory.CreateRandomProduct();
                _builder.WithProductLine(1, grossPerLine / rate.Value, rate.Value, rate.Key, product);
            }
        }

        public void ConstructWithSettlement(
            decimal gross,
            decimal taxRate, 
            PaymentCategory paymentCategory, 
            decimal discountRate
        )
        {
            var product = ProductFactory.CreateCocaCola();
            
            ConstructAnonymousWithoutLines();

            _builder
                .WithPayment(gross - gross * discountRate, paymentCategory)
                .WithProductLineSettlement(
                    product,
                    TaxesCategory.High,
                    gross / taxRate,
                    taxRate,
                    -discountRate * gross
                );
        }

        /// <summary>
        /// Construct a return invoice for everything contained in the given invoice.
        /// </summary>
        /// <param name="invoice"></param>
        public void ConstructReturn(Invoice invoice)
        {
            ConstructAnonymousWithoutLines();

            foreach (var line in invoice.Lines)
            {
                _builder.WithReturnLine(invoice, line);
            }

            foreach (var payment in invoice.Payments)
            {
                _builder.WithPayment(-payment.Amount, payment.Category);   
            }
        }
    }
}