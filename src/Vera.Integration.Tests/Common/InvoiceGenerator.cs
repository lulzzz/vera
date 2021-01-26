using System;
using Bogus;
using Google.Protobuf.WellKnownTypes;
using Vera.Grpc;

namespace Vera.Integration.Tests.Common
{
    public class InvoiceGenerator
    {
        private readonly Faker _faker;

        public InvoiceGenerator(Faker faker)
        {
            _faker = faker;
        }

        public Invoice CreateInvoiceWithCustomerAndSingleProduct(string account)
        {
            return new()
            {
                Account = account,
                SystemId = "1",
                TerminalId = "616.1337",
                Remark = "hello world",
                Customer = new Customer
                {
                    SystemId = "1",
                    Email = _faker.Person.Email,
                    FirstName = _faker.Person.FirstName,
                    LastName = _faker.Person.LastName,
                    BillingAddress = CreateAddress(),
                    ShippingAddress = CreateAddress(),
                },
                Employee = new Employee
                {
                    SystemId = "1",
                    FirstName = _faker.Person.FirstName,
                    LastName = _faker.Person.LastName
                },
                Manual = false,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                BillingAddress = CreateAddress(),
                ShippingAddress = CreateAddress(),
                Supplier = new Supplier
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
                        Category = Payment.Types.Category.Cash,
                        Code = "CASH",
                        Description = "Cash",
                        Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
                    }
                },
                Lines =
                {
                    new InvoiceLine
                    {
                        Description = "Coca cola",
                        Product = new Product
                        {
                            Group = Product.Types.Group.Other,
                            Code = "COCA",
                            Description = "Coca cola"
                        },
                        Quantity = 1,
                        Type = InvoiceLine.Types.Type.Goods,
                        Unit = "EA",
                        UnitPrice = 1.99m / 1.21m,
                        Gross = 1.99m / 1.21m,
                        Net = 1.99m,
                        Tax = new TaxValue
                        {
                            Code = "HIGH",
                            Rate = 1.21m
                        }
                    }
                }
            };
        }

        private Grpc.Shared.Address CreateAddress()
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