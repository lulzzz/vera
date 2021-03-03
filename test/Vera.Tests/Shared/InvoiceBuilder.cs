using System;
using System.Collections.Generic;
using Bogus;
using Vera.Models;

namespace Vera.Tests.Shared
{
    public class InvoiceBuilder
    {
        private readonly Faker _faker;
        private Invoice _invoice;

        public InvoiceBuilder(Faker faker)
        {
            _faker = faker;
            
            Reset();
        }

        public InvoiceBuilder WithTerminal(string terminal)
        {
            _invoice.TerminalId = terminal;

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

        public InvoiceBuilder WithSupplier()
        {
            _invoice.Supplier = new Billable
            {
                SystemId = _faker.Random.Number(1, int.MaxValue).ToString(),
                Name = _faker.Company.CompanyName(),
                Address = CreateAddress()
            };
            
            return this;
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

            return WithProductLine(new Product
            {
                SystemId = _faker.Random.Number(1, int.MaxValue).ToString(),
                Type = ProductType.Goods,
                Code = _faker.Commerce.Ean13(),
                Description = _faker.Commerce.Product()
            }, TaxesCategory.High, amount, taxRate);
        }

        public InvoiceBuilder WithProductLine(
            Product product, 
            TaxesCategory taxCategory, 
            decimal amountInTax, 
            decimal taxRate)
        {
            _invoice.Lines.Add(new InvoiceLine
            {
                Description = product.Description,
                Product = product,
                Quantity = 1,
                UnitPrice = amountInTax / taxRate,
                Gross = amountInTax,
                Net = amountInTax / taxRate,
                Taxes = new Taxes
                {
                    Category = taxCategory,
                    Code = taxCategory.ToString().ToUpper(),
                    Rate = taxRate
                }
            });
            
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
    }
}