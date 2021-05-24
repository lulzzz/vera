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

        [Fact]
        public async Task Should_open_close_period()
        {
            var accountContext = new AccountContext
            {
                AccountName = _faker.Company.CompanyName(),
                Certification = "123",
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
            Assert.Equal(DateTime.MinValue, period.Closing.ToDateTime());

            var closePeriodRequest = new ClosePeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId
            };
            await client.Period.ClosePeriodAsync(closePeriodRequest, client.AuthorizedMetadata);

            period = await client.Period.GetAsync(getPeriodRequest, client.AuthorizedMetadata);

            Assert.NotEqual(DateTime.MinValue, period.Closing.ToDateTime());
        }

        [Fact]
        public async Task Should_close_period_with_registers()
        {
            var client = await _setup.CreateClient(Portugal.Constants.Account);

            var openPeriodRequest = new OpenPeriodRequest { SupplierSystemId = client.SupplierSystemId };
            var openPeriodReply = await client.Period.OpenPeriodAsync(openPeriodRequest, client.AuthorizedMetadata);

            var createRegisterRequest = new CreateRegisterRequest()
            {
                SupplierSystemId = client.SupplierSystemId,
            };

            var register = await client.Register.CreateRegisterAsync(createRegisterRequest, client.AuthorizedMetadata);

            var openRegisterRequest = new OpenRegisterRequest
            {
                OpeningAmount = 10m,
                SupplierSystemId = client.SupplierSystemId,
                RegisterId = register.Id
            };

            var openRegisterReply = await client.Register.OpenRegisterAsync(openRegisterRequest, client.AuthorizedMetadata);

            var registerEntries = new Dictionary<string, ClosePeriodRegisterEntry>
            {
                {
                    openRegisterReply.Id,
                    new ClosePeriodRegisterEntry
                    {
                        Id = openRegisterReply.Id,
                        ClosingAmount = 100m,
                    }
                }
            };
            var closePeriodRequest = new ClosePeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId,
                Registers = { registerEntries }
            };

            await client.Period.ClosePeriodAsync(closePeriodRequest, client.AuthorizedMetadata);

            var getPeriodRequest = new GetPeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId
            };
            var period = await client.Period.GetAsync(getPeriodRequest, client.AuthorizedMetadata);

            Assert.NotEqual(DateTime.MinValue, period.Closing.ToDateTime());
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
            };

            var register = await client.Register.CreateRegisterAsync(createRegisterRequest, client.AuthorizedMetadata);

            var openRegisterRequest = new OpenRegisterRequest
            {
                OpeningAmount = 10m,
                SupplierSystemId = client.SupplierSystemId,
                RegisterId = register.Id
            };

            var openRegisterReply = await client.Register.OpenRegisterAsync(openRegisterRequest, client.AuthorizedMetadata);
            var getPeriodRequest = new GetPeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId
            };

            var scenarios = new List<ClosePeriodRequest>();
            var registerEntries = new Dictionary<string, ClosePeriodRegisterEntry>
            {
                {
                    openRegisterReply.Id,
                    new ClosePeriodRegisterEntry
                    {
                        Id = openRegisterReply.Id,
                        ClosingAmount = 100m,
                    }
                },
                {
                    openPeriodReply.Id,
                    new ClosePeriodRegisterEntry
                    {
                        Id = openPeriodReply.Id,
                        ClosingAmount = 200m
                    }
                }
            };

            //no registers
            var closePeriodRequest1 = new ClosePeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId
            };
            scenarios.Add(closePeriodRequest1);

            //more registers
            var closePeriodRequest2 = new ClosePeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId,
                Registers = { registerEntries }
            };
            scenarios.Add(closePeriodRequest2);

            var lastRegisterItem = new Dictionary<string, ClosePeriodRegisterEntry> {
                { registerEntries.Last().Key,registerEntries.Last().Value}
            };

            //wrong register
            var closePeriodRequest3 = new ClosePeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId,
                Registers = { lastRegisterItem }
            };
            scenarios.Add(closePeriodRequest3);

            foreach (var closeRequest in scenarios)
            {
                try
                {
                    await client.Period.ClosePeriodAsync(closeRequest, client.AuthorizedMetadata);

                    //not reachable
                    Assert.True(false);

                }
                catch (RpcException ex)
                {
                    Assert.True(ex.StatusCode == StatusCode.FailedPrecondition);

                    var period = await client.Period.GetAsync(getPeriodRequest, client.AuthorizedMetadata);

                    Assert.Equal(DateTime.MinValue, period.Closing.ToDateTime());
                }
            }
        }
    }
}
