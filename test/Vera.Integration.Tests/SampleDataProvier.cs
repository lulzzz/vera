using Bogus;
using System.Threading.Tasks;
using Vera.Grpc;

namespace Vera.Integration.Tests
{
    public class SampleDataProvier
    {
        private Faker _faker;
        private SetupClient _client;

        public SampleDataProvier(SetupClient client)
        {
            _faker = new Faker();
            _client = client;
        }

        public async Task<Grpc.Shared.Supplier> CreateSupplier(string systemId = null)
        {
            var supplier = new Grpc.Shared.Supplier
            {
                Name = _faker.Name.FullName(),
                RegistrationNumber = _faker.Random.AlphaNumeric(10),
                TaxRegistrationNumber = _faker.Random.AlphaNumeric(10),
                SystemId = systemId ?? _faker.Random.AlphaNumeric(10),
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
            await _client.Supplier.CreateAsync(createRequest, _client.AuthorizedMetadata);

            var getRequest = new GetSupplierRequest { SystemId = supplier.SystemId };
            var getReplySupplier = await _client.Supplier.GetAsync(getRequest, _client.AuthorizedMetadata);

            return getReplySupplier;
        }

        public async Task<string> CreateOpenPeriod(string supplierSystemId)
        {
            var openPeriodRequest = new OpenPeriodRequest { SupplierSystemId = supplierSystemId };
            var openPeriodReply = await _client.Period.OpenPeriodAsync(openPeriodRequest, _client.AuthorizedMetadata);

            return openPeriodReply.Id;
        }
    }
}
