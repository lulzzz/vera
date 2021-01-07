using System;
using System.Threading.Tasks;
using Bogus;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Vera.Grpc;
using Xunit;

namespace Vera.Integration.Tests.Portugal
{
    // TODO: extract parts of this class
    // can re-use different invoice scenario's and verify them
    // in the specific test classes for portugal france, etc.
    public class InvoiceTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly GrpcChannel _channel;
        private readonly Faker _faker;

        public InvoiceTests(ApiWebApplicationFactory fixture)
        {
            var client = fixture.CreateClient();

            _channel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions
            {
                HttpClient = client
            });

            _faker = new Faker();
        }

        [Fact]
        public async Task Should_be_able_to_create_an_invoice()
        {
            var setup = new Setup(_channel, _faker);
            var token = await setup.CreateLogin();
            var account = await setup.CreateAccount();

            await ConfigureAccount(setup, account);

            var createInvoiceRequest = new CreateInvoiceRequest
            {
                Invoice = CreateInvoice(account)
            };

            var invoiceService = new InvoiceService.InvoiceServiceClient(_channel);
            using var createInvoiceCall = invoiceService.CreateAsync(createInvoiceRequest, setup.CreateAuthorizedMetadata());

            var createInvoiceReply = await createInvoiceCall.ResponseAsync;

            Assert.NotNull(createInvoiceReply.Number);
            Assert.NotNull(createInvoiceReply.Signature);
        }

        private static async Task ConfigureAccount(Setup setup, string account)
        {
            const string pk = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQC96HMXSV2MuX4VmVbj5y0oZeCZzjXDaZLvi0U+S1sAt/VfTRT7
cOgHm2VbLo+o5nx24UbIxH0osATZBUTYZCEiwRnSDp0cyse1MmbsfmCd9KYWWyc7
I9I63Jn0lwkCJWp+hU2SMrumRpKqJ0Lza+T4S5matp0wikQr/RZorNjN4QIDAQAB
AoGAPirwMjlUJJM8kTmHVkgBYm4nXnJA612OOlivLDti6RNPggkryzwk2Qin33eY
k8QQDqKkl2irSDyG+bxd0zDEH5nwCmT59pvTQ1t36E6mtPgg4wfpPyy86sCi1ecJ
UwoZjdpDnwfAWDOjgCgZiu1tr9L9uw7hG8fn+/99kSSmxTkCQQDm0ISO2CASWc0X
Y2gKU//ZZXBymDjnq+VvKBRAyO2lpfc6LnL0op7NnbDdHcUJO21nzX9pr+wJqLaN
W/feWYpXAkEA0qFD5KqfDYJ21VxMRLZ+SNAD5zgIGx6VajpcIFMZfmZ6H0OShKBY
KQlbYS22baIClrHawyDU/jARO8eC2JG2hwJAIRd8Kc6qqnbdhKDn5bMtV0nH2WYh
onVuq4UfgjpMeBdXXqwSJyi5g9k75jfCbBRtFxjLT6e9O5VItvOckfBceQJBAIx1
G+hF21DP+lyncvizVZ1Kkf/DfqxPBcZT6pFnuO1weumUTwWAQ6oB4lz4ddnAGsfR
DJfosgBbn3Jkxh2TdcsCQQDhjs8VIbJITJtCvsfmi0SykOyuDvFZlEWqy/io12ge
tq4HEcmINDkh3fy0/V5XRqzAmGlH6dgxPMgEdddzdRrl
-----END RSA PRIVATE KEY-----";

            await setup.AccountClient.CreateOrUpdateConfigurationAsync(new AccountConfigurationRequest
            {
                Id = account,
                Fields =
                {
                    {"PrivateKey", pk},
                    {"ProductCompanyTaxId", "123 123 123"},
                    {"SocialCapital", "1234"}
                }
            }, setup.CreateAuthorizedMetadata());
        }

        private Invoice CreateInvoice(string account)
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