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

            return WithProductLine(1, amount, taxRate, TaxesCategory.High, new Product
            {
                SystemId = _faker.Random.Number(1, int.MaxValue).ToString(),
                Type = ProductType.Goods,
                Code = _faker.Commerce.Ean13(),
                Description = _faker.Commerce.Product()
            });
        }

        public InvoiceBuilder WithProductLine(int quantity,
            decimal unitPrice,
            decimal taxRate,
            TaxesCategory taxCategory,
            Product product)
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

        public InvoiceBuilder WithReturnLine(
            Product product,
            TaxesCategory taxCategory,
            decimal unitPrice,
            int quantity,
            decimal taxRate,
            Invoice originalInvoice)
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
                CreditReference = new CreditReference
                {
                    Number = originalInvoice.Number,
                    Reason = "Return invoice"
                }
        });

            return this;
        }


        public InvoiceBuilder WithSettlement(decimal amountInTax)
        {
            _invoice.Settlements.Add(new Settlement
            {
                Amount = amountInTax,
                SystemId = Guid.NewGuid().ToString(),
                Description = "Description"
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

        public Invoice Result => _invoice;
    }
}