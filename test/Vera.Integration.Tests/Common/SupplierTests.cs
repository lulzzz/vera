using Bogus;
using Grpc.Core;
using System.Threading.Tasks;
using Vera.Grpc;
using Xunit;

namespace Vera.Integration.Tests.Common
{
    public class SupplierTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Faker _faker;
        private readonly Setup _setup;

        public SupplierTests(ApiWebApplicationFactory fixture)
        {
            _faker = new Faker();
            _setup = fixture.CreateSetup();
        }

        [Fact]
        public async Task Should_create_supplier()
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

            Assert.NotNull(getReplySupplier);
            Assert.NotNull(getReplySupplier.Address);
        }

        [Fact]
        public async Task Should_update_supplier()
        {
            var accountContext = new AccountContext
            {
                CompanyName = _faker.Company.CompanyName(),
                AccountName = _faker.Company.CompanyName(),
                Certification = "11234"
            };

            var client = await _setup.CreateClient(accountContext);

            var supplier = new Grpc.Shared.Supplier
            {
                Name = _faker.Name.FullName(),
                RegistrationNumber = _faker.Random.AlphaNumeric(10),
                TaxRegistrationNumber = _faker.Random.AlphaNumeric(10),
                SystemId = _faker.Random.AlphaNumeric(10)
            };

            var createRequest = new CreateSupplierRequest { Supplier = supplier };
            await client.Supplier.CreateAsync(createRequest, client.AuthorizedMetadata);

            var getRequest = new GetSupplierRequest { SystemId = supplier.SystemId };
            var getReplySupplier = await client.Supplier.GetAsync(getRequest, client.AuthorizedMetadata);

            Assert.Equal(getReplySupplier.Name, supplier.Name);
            Assert.Equal(getReplySupplier.RegistrationNumber, supplier.RegistrationNumber);
            Assert.Equal(getReplySupplier.TaxRegistrationNumber, supplier.TaxRegistrationNumber);
            Assert.Equal(getReplySupplier.SystemId, supplier.SystemId);

            supplier.Name = _faker.Name.FullName();
            supplier.RegistrationNumber = _faker.Random.AlphaNumeric(10);
            supplier.TaxRegistrationNumber = _faker.Random.AlphaNumeric(10);
            supplier.Address = new Grpc.Shared.Address
            {
                Street = _faker.Address.StreetAddress()
            };

            var request = new UpdateSupplierRequest { SystemId = supplier.SystemId, Supplier = supplier };
            var updateReplySupplier = await client.Supplier.UpdateAsync(request, client.AuthorizedMetadata);

            Assert.Equal(updateReplySupplier.Name, supplier.Name);
            Assert.Equal(updateReplySupplier.RegistrationNumber, supplier.RegistrationNumber);
            Assert.Equal(updateReplySupplier.TaxRegistrationNumber, supplier.TaxRegistrationNumber);
            Assert.Equal(updateReplySupplier.SystemId, supplier.SystemId);
            Assert.Equal(updateReplySupplier.Address.Street, supplier.Address.Street);
        }

        [Fact]
        public async Task Should_delete_supplier()
        {
            var accountContext = new AccountContext
            {
                CompanyName = _faker.Company.CompanyName(),
                AccountName = _faker.Company.CompanyName(),
                Certification = "11234"
            };

            var client = await _setup.CreateClient(accountContext);

            var supplier = new Grpc.Shared.Supplier
            {
                Name = _faker.Name.FullName(),
                RegistrationNumber = _faker.Random.AlphaNumeric(10),
                TaxRegistrationNumber = _faker.Random.AlphaNumeric(10),
                SystemId = _faker.Random.AlphaNumeric(10)
            };

            var createRequest = new CreateSupplierRequest { Supplier = supplier };
            await client.Supplier.CreateAsync(createRequest, client.AuthorizedMetadata);

            var getRequest = new GetSupplierRequest { SystemId = supplier.SystemId };
            var getReplySupplier = await client.Supplier.GetAsync(getRequest, client.AuthorizedMetadata);

            Assert.NotNull(getReplySupplier);
            Assert.Equal(getReplySupplier.Name, supplier.Name);
            Assert.Equal(getReplySupplier.RegistrationNumber, supplier.RegistrationNumber);
            Assert.Equal(getReplySupplier.TaxRegistrationNumber, supplier.TaxRegistrationNumber);
            Assert.Equal(getReplySupplier.Address, supplier.Address);
            Assert.Equal(getReplySupplier.SystemId, supplier.SystemId);

            var request = new DeleteSupplierRequest { SystemId = supplier.SystemId };
            
            await client.Supplier.DeleteAsync(request, client.AuthorizedMetadata);

            try
            {
                await client.Supplier.GetAsync(getRequest, client.AuthorizedMetadata);
            }
            catch(RpcException ex)
            {
                Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
            }
        }
    }
}
