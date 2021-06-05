using System;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Google.Protobuf.WellKnownTypes;
using Vera.Grpc;
using Xunit;

namespace Vera.Integration.Tests.Common
{
    public class EventLogTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Faker _faker;
        private readonly Setup _setup;

        public EventLogTests(ApiWebApplicationFactory fixture)
        {
            _faker = new Faker();
            _setup = fixture.CreateSetup();
        }

        [Fact]

        public async Task Should_create_event_log()
        {
            var client = await EnsureClient();
            var supplier = await EnsureSupplier(client);

            var eventLog = new EventLog
            {
                SupplierSystemId = supplier.SystemId,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                Type = EventLogType.ReceiptPrinted
            };

            var createRequest = new CreateEventLogRequest
            {
                Eventlog = eventLog
            };

            var reply = await client.EventLog.CreateAsync(createRequest, client.AuthorizedMetadata);
            Assert.NotNull(reply);

            var listAllEventsReply =
                await client.EventLog.ListAsync(new ListEventLogRequest
                {
                    SupplierSystemId = supplier.SystemId
                }, client.AuthorizedMetadata);

            Assert.NotNull(listAllEventsReply);
            var matchingEventLog = listAllEventsReply.EventLogs.SingleOrDefault(x => x.Id == reply.Id);

            Assert.NotNull(matchingEventLog);
            Assert.Equal(EventLogType.ReceiptPrinted, matchingEventLog.Type);
            Assert.Equal(supplier.SystemId, matchingEventLog.SupplierSystemId);
        }

        private async Task<SetupClient> EnsureClient()
        {
            var accountContext = new AccountContext
            {
                AccountName = _faker.Company.CompanyName(),
                Certification = "123"
            };


            var client = await _setup.CreateClient(accountContext);

            return client;
        }

        private async Task<Grpc.Shared.Supplier> EnsureSupplier(SetupClient client)
        {
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

            Assert.NotNull(getReplySupplier);
            Assert.NotNull(getReplySupplier.Address);

            return getReplySupplier;
        }
    }
}
