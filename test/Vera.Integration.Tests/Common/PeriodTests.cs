using Bogus;
using Google.Protobuf.Collections;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vera.Grpc;
using Vera.Tests.TestParameters;
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

        [Theory]
        [ClassData(typeof(CertificationKeys))]
        public async Task Should_open_close_period(string certification)
        {
            var accountContext = new AccountContext
            {
                AccountName = _faker.Company.CompanyName(),
                Certification = certification,
                SupplierSystemId = _faker.Random.AlphaNumeric(10)
            };

            var client = await _setup.CreateClient(accountContext);

            var openPeriodRequest = new OpenPeriodRequest { SupplierSystemId = client.SupplierSystemId };
            var openPeriodReply = await client.Period.OpenPeriodAsync(openPeriodRequest, client.AuthorizedMetadata);

            var getPeriodRequest = new GetPeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId
            };
            var period = await client.Period.GetAsync(getPeriodRequest, client.AuthorizedMetadata);

            Assert.NotNull(period);
            Assert.NotNull(period.Opening);
            Assert.NotNull(period.SupplierSystemId);
            Assert.Null(period.Closing);

            var closePeriodRequest = new ClosePeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId
            };

            await client.Period.ClosePeriodAsync(closePeriodRequest, client.AuthorizedMetadata);

            period = await client.Period.GetAsync(getPeriodRequest, client.AuthorizedMetadata);

            Assert.NotNull(period.Closing);
        }

        [Fact]
        public async Task Should_close_period_with_registers()
        {
            var client = await _setup.CreateClient(Portugal.Constants.Account);

            var createRegisterRequest = new CreateRegisterRequest()
            {
                SupplierSystemId = client.SupplierSystemId,
                SystemId = "123"
            };

            await client.Register.CreateRegisterAsync(createRegisterRequest, client.AuthorizedMetadata);

            var openPeriodRequest = new OpenPeriodRequest
            {
                SupplierSystemId = client.SupplierSystemId
            };

            var openPeriodReply = await client.Period.OpenPeriodAsync(openPeriodRequest, client.AuthorizedMetadata);

            var openRegisterRequest = new OpenRegisterRequest
            {
                OpeningAmount = 10m,
                SupplierSystemId = client.SupplierSystemId,
                RegisterSystemId = createRegisterRequest.SystemId
            };

            await client.Period.OpenRegisterAsync(openRegisterRequest, client.AuthorizedMetadata);

            var closePeriodRequest = new ClosePeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId,
                Registers =
                {
                    new RegisterCloseEntry
                    {
                        SystemId = createRegisterRequest.SystemId,
                        Amount = 100m
                    }
                }
            };

            await client.Period.ClosePeriodAsync(closePeriodRequest, client.AuthorizedMetadata);

            var getPeriodRequest = new GetPeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId
            };

            var period = await client.Period.GetAsync(getPeriodRequest, client.AuthorizedMetadata);

            Assert.NotNull(period.Closing);
        }

        [Theory]
        [ClassData(typeof(CertificationKeys))]
        public async Task Should_not_allow_closing_period(string certification)
        {
            var accountContext = new AccountContext
            {
                AccountName = _faker.Company.CompanyName(),
                Certification = certification,
                SupplierSystemId = _faker.Random.AlphaNumeric(10)
            };

            var client = await _setup.CreateClient(accountContext);

            var openPeriodRequest = new OpenPeriodRequest { SupplierSystemId = client.SupplierSystemId };
            var openPeriodReply = await client.Period.OpenPeriodAsync(openPeriodRequest, client.AuthorizedMetadata);

            var createRegisterRequest = new CreateRegisterRequest()
            {
                SupplierSystemId = client.SupplierSystemId,
                SystemId = "123"
            };

            await client.Register.CreateRegisterAsync(createRegisterRequest, client.AuthorizedMetadata);

            var openRegisterRequest = new OpenRegisterRequest
            {
                OpeningAmount = 10m,
                SupplierSystemId = client.SupplierSystemId,
                RegisterSystemId = createRegisterRequest.SystemId
            };

            await client.Period.OpenRegisterAsync(openRegisterRequest, client.AuthorizedMetadata);

            var getPeriodRequest = new GetPeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId
            };

            var scenarios = new List<ClosePeriodRequest>();

            var closePeriodWithNoRegistersRequest = new ClosePeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId
            };

            scenarios.Add(closePeriodWithNoRegistersRequest);

            var closePeriodWithTooManyRegistersRequest = new ClosePeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId,
                Registers =
                {
                    new RegisterCloseEntry
                    {
                        SystemId = createRegisterRequest.SystemId,
                        Amount = 100m
                    },
                    new RegisterCloseEntry
                    {
                        SystemId = "321",
                        Amount = 200m
                    },
                }
            };

            scenarios.Add(closePeriodWithTooManyRegistersRequest);

            var closePeriodWithWrongRegisterRequest = new ClosePeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId,
                Registers =
                {
                    new RegisterCloseEntry
                    {
                        SystemId = "321",
                        Amount = 200m
                    },
                }
            };

            scenarios.Add(closePeriodWithWrongRegisterRequest);

            foreach (var scenario in scenarios)
            {
                try
                {
                    await client.Period.ClosePeriodAsync(scenario, client.AuthorizedMetadata);

                    // Should not be reached
                    Assert.True(false);

                }
                catch (RpcException)
                {
                    var period = await client.Period.GetAsync(getPeriodRequest, client.AuthorizedMetadata);

                    Assert.Null(period.Closing);
                }
            }
        }
    }
}
