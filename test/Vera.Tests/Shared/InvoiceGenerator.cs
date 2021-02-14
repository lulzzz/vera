using System;
using Bogus;
using Vera.Models;

namespace Vera.Tests.Shared
{
    public class InvoiceGenerator
    {
        private readonly Faker _faker;

        public InvoiceGenerator(Faker faker)
        {
            _faker = faker;
        }

        public Invoice CreateAnonymousWithSingleProduct(string account)
        {
            // TODO(kevin): convert to builder pattern

            return new()
            {
                AccountId = Guid.Parse(account),
                SystemId = "1",
                TerminalId = "616.1337",
                Remark = "hello world",
                // Customer = new Customer
                // {
                //     SystemId = "1",
                //     Email = _faker.Person.Email,
                //     FirstName = _faker.Person.FirstName,
                //     LastName = _faker.Person.LastName,
                //     BillingAddress = CreateAddress(),
                //     ShippingAddress = CreateAddress(),
                // },
                Employee = new Employee
                {
                    SystemId = "1",
                    FirstName = _faker.Person.FirstName,
                    LastName = _faker.Person.LastName
                },
                Manual = false,
                Date = DateTime.UtcNow,
                Supplier = new Billable
                {
                    SystemId = "1",
                    Name = _faker.Company.CompanyName(),
                    Address = CreateAddress()
                },
                Payments =
                {
                    new Payment
                    {
                        Amount = 1.99m,
                        Category = PaymentCategory.Cash,
                        Description = "Cash",
                        Date = DateTime.UtcNow,
                        SystemId = "1"
                    }
                },
                Lines =
                {
                    new InvoiceLine
                    {
                        Description = "Coca cola",
                        Product = new Product
                        {
                            Code = "COCA",
                            Description = "Coca cola"
                        },
                        Type = InvoiceLineType.Goods,
                        Quantity = 1,
                        UnitPrice = 1.99m / 1.21m,
                        Gross = 1.99m,
                        Net = 1.99m / 1.21m,
                        Taxes = new Taxes
                        {
                            Category = TaxesCategory.High,
                            Code = "HIGH",
                            Rate = 1.21m
                        }
                    }
                }
            };
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