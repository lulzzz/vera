using Bogus;
using System;
using System.Threading.Tasks;
using Vera.Grpc;
using Xunit;

namespace Vera.Integration.Tests.Common
{
    public class PeriodTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Faker _faker;
        private readonly Setup _setup;

        public PeriodTests(ApiWebApplicationFactory fixture)
        {
            _faker = new Faker();
            _setup = fixture.CreateSetup();
        }

        [Fact]
        public async Task Should_open_close_period()
        {
            var accountContext = new AccountContext
            {
                CompanyName = _faker.Company.CompanyName(),
                AccountName = _faker.Company.CompanyName(),
                Certification = "123"
            };

            var client = await _setup.CreateClient(accountContext);

            var supplier = new Grpc.Shared.Supplier
            {
                Name = _faker.Name.FullName(),
                RegistrationNumber = _faker.Random.AlphaNumeric(10),
                TaxRegistrationNumber = _faker.Random.AlphaNumeric(10),
                SystemId = _faker.Random.AlphaNumeric(10),
                Address = new Grpc.Shared.Address
                {
                    City = _faker.Address.City(),
                    Country = _faker.Address.Country(),
                    Number = _faker.Address.BuildingNumber(),
                    PostalCode = _faker.Address.ZipCode(),
                    Region = _faker.Address.County(),
                    Street = _faker.Address.StreetAddress()
                }
            };

            var createRequest = new CreateSupplierRequest { Supplier = supplier };
            var reply = await client.Supplier.CreateAsync(createRequest, client.AuthorizedMetadata);

            Assert.NotNull(reply);

            var getRequest = new GetSupplierRequest { SystemId = supplier.SystemId };
            var getReplySupplier = await client.Supplier.GetAsync(getRequest, client.AuthorizedMetadata);

            var openPeriodRequest = new OpenPeriodRequest { SupplierSystemId = getReplySupplier.SystemId };
            var openPeriodReply = await client.Period.OpenPeriodAsync(openPeriodRequest, client.AuthorizedMetadata);

            var getPeriodRequest = new GetPeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = supplier.SystemId
            };
            var period = await client.Period.GetAsync(getPeriodRequest);

            Assert.NotNull(period);
            Assert.NotNull(period.Opening);
            Assert.NotNull(period.SupplierSystemId);
            Assert.Equal(DateTime.MinValue, period.Closing.ToDateTime());

            var closePeriodRequest = new ClosePeriodRequest 
            { 
                Id = openPeriodReply.Id,
                SupplierSystemId = supplier.SystemId
            };
            await client.Period.ClosePeriodAsync(closePeriodRequest, client.AuthorizedMetadata);

            period = await client.Period.GetAsync(getPeriodRequest);

            Assert.NotNull(period.Closing);
        }
    }
}
