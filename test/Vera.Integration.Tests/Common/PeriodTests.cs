using Bogus;
using Google.Protobuf.Collections;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vera.Grpc;
using Vera.Grpc.Shared;
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

            var openRegisterRequest = new OpenRegisterRequest
            {
                OpeningAmount = 10m,
                SupplierSystemId = client.SupplierSystemId
            };

            var openRegisterReply = await client.Register.OpenRegisterAsync(openRegisterRequest, client.AuthorizedMetadata);

            var registers = new RepeatedField<Register>
            {
                new Register
                {
                    Id = openRegisterReply.Id,
                    ClosingAmount = 100m
                }
            };
            var closePeriodRequest = new ClosePeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId,
                Registers = { registers }
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

        [Fact]
        public async Task Should_not_allow_closing_period()
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

            var openRegisterRequest = new OpenRegisterRequest
            {
                OpeningAmount = 10m,
                SupplierSystemId = client.SupplierSystemId
            };

            var openRegisterReply = await client.Register.OpenRegisterAsync(openRegisterRequest, client.AuthorizedMetadata);
            var getPeriodRequest = new GetPeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId
            };

            var scenarios = new List<ClosePeriodRequest>();
            var registers = new RepeatedField<Register>
            {
                new Register
                {
                    Id = openRegisterReply.Id,
                    ClosingAmount = 100m
                },
                new Register
                {
                    Id = openPeriodReply.Id,
                    ClosingAmount = 200m
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
                Registers = {registers}
            };
            scenarios.Add(closePeriodRequest2);

            //wrong register
            var closePeriodRequest3 = new ClosePeriodRequest
            {
                Id = openPeriodReply.Id,
                SupplierSystemId = client.SupplierSystemId,
                Registers = { registers.Skip(1) }
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
