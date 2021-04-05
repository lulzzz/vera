using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Microsoft.Extensions.Azure;
using Vera.Invoices;
using Vera.Models;

namespace Vera.Tests.Shared
{
    public class InvoiceBuilder
    {
        private readonly Faker _faker;
        private Invoice _invoice;

        public InvoiceBuilder()
        {
            _faker = new Faker();

            Reset();
        }

        public InvoiceBuilder WithAccount(Guid accountId)
        {
            _invoice.AccountId = accountId;
            return this;
        }

        public InvoiceBuilder WithTerminal(string terminal)
        {
            _invoice.RegisterId = terminal;
            return this;
        }

        public InvoiceBuilder WithEmployee()
        {
            _invoice.Employee = new Employee
            {
                SystemId = _faker.Random.Number(1, int.MaxValue).ToString(),
                FirstName = _faker.Person.FirstName,
                LastName = _faker.Person.LastName
            };

            return this;
        }

        public InvoiceBuilder WithSupplier(string supplierSystemId)
        {
            _invoice.Supplier = new Supplier
            {
                SystemId = supplierSystemId,
                Name = _faker.Company.CompanyName(),
                Address = CreateAddress()
            };

            return this;
        }
        
        public InvoiceBuilder WithPayment(PaymentCategory category)
        {
            var calculator = new InvoiceTotalsCalculator();
            var totals = calculator.Calculate(_invoice);
            
            _invoice.Payments.Clear();

            return WithPayment(totals.Gross, category);
        }

        public InvoiceBuilder WithPayment(decimal amount, PaymentCategory category)
        {
            _invoice.Payments.Add(new Payment
            {
                Amount = amount,
                Category = category,
                Description = category.ToString(),
                Date = DateTime.UtcNow,
                SystemId = (_invoice.Payments.Count + 1).ToString()
            });

            return this;
        }

        public InvoiceBuilder WithAmount(decimal amount, decimal taxRate)
        {
            _invoice.Lines.Clear();

            return WithProductLine(1, amount / taxRate, taxRate, TaxesCategory.High, new Product
            {
                SystemId = _faker.Random.Number(1, int.MaxValue).ToString(),
                Type = ProductType.Goods,
                Code = _faker.Commerce.Ean13(),
                Description = _faker.Commerce.Product()
            });
        }

        public InvoiceBuilder WithProductLine(
            int quantity,
            decimal unitPrice,
            decimal taxRate,
            TaxesCategory taxCategory,
            Product product
        )
        {
            _invoice.Lines.Add(new InvoiceLine
            {
                Description = product.Description,
                Product = product,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Taxes = new Taxes
                {
                    Category = taxCategory,
                    Code = taxCategory.ToString().ToUpper(),
                    Rate = taxRate
                },
            });

            return this;
        }

        public InvoiceBuilder WithProductLineSettlement(
           Product product,
           TaxesCategory taxCategory,
           decimal unitPrice,
           decimal taxRate,
           decimal settlementAmountInTax)
        {
            _invoice.Lines.Add(new InvoiceLine
            {
                Description = product.Description,
                Product = product,
                Quantity = 1,
                UnitPrice = unitPrice,
                Taxes = new Taxes
                {
                    Category = taxCategory,
                    Code = taxCategory.ToString().ToUpper(),
                    Rate = taxRate
                },
                Settlements = new List<Settlement>()
                {
                    new()
                    {
                        Amount = settlementAmountInTax,
                        SystemId = Guid.NewGuid().ToString(),
                        Description = "Description"
                    }
                }
            });

            return this;
        }

        public InvoiceBuilder WithReturnLine(Invoice invoice, InvoiceLine lineToReturn)
        {
            _invoice.Lines.Add(new InvoiceLine
            {
                Description = lineToReturn.Description,
                Product = lineToReturn.Product,
                Quantity = -lineToReturn.Quantity,
                UnitPrice = lineToReturn.UnitPrice,
                Taxes = lineToReturn.Taxes,
                CreditReference = new CreditReference
                {
                    Number = invoice.Number,
                    Reason = "Return invoice"
                }
            });

            return this;
        }

        public InvoiceBuilder WithRemark(string remark)
        {
            _invoice.Remark = remark;
            return this;
        }

        public InvoiceBuilder WithSignature(Signature signature)
        {
            _invoice.Signature = signature;
            return this;
        }

        public InvoiceBuilder Reset()
        {
            _invoice = new Invoice
            {
                SystemId = _faker.Random.Number(1, int.MaxValue).ToString(),
                Date = DateTime.UtcNow,
                Lines = new List<InvoiceLine>(),
                Payments = new List<Payment>()
            };

            return this;
        }

        public Invoice Build()
        {
            return _invoice;
        }

        private Address CreateAddress()
        {
            return new()
            {
                City = _faker.Address.City(),
                Country = _faker.Address.CountryCode(),
                Number = _faker.Address.BuildingNumber(),
                Region = _faker.Address.StateAbbr(),
                PostalCode = _faker.Address.ZipCode(),
                Street = _faker.Address.StreetName()
            };
        }

        public Invoice Result => _invoice;
    }
}