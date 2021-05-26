using Bogus;
using System.Threading.Tasks;
using Vera.Grpc;
using Vera.Grpc.Shared;
using Vera.Tests.TestParameters;
using Xunit;

namespace Vera.Integration.Tests.Common
{
    public class RegisterTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Faker _faker;
        private readonly Setup _setup;

        public RegisterTests(ApiWebApplicationFactory fixture)
        {
            _faker = new Faker();
            _setup = fixture.CreateSetup();
        }

        [Theory]
        [ClassData(typeof(RegisterOpenStatusCertificationKeys))]
        public async Task Should_have_register_status_open(string certification)
        {
            var accountContext = new AccountContext
            {
                AccountName = _faker.Company.CompanyName(),
                Certification = certification,
                SupplierSystemId = _faker.Random.AlphaNumeric(10)
            };

            var client = await _setup.CreateClient(accountContext);

            var createRegisterRequest = new CreateRegisterRequest()
            {
                SupplierSystemId = client.SupplierSystemId,
            };

            var register = await client.Register.CreateRegisterAsync(createRegisterRequest, client.AuthorizedMetadata);

            var getRegisterRequest = new GetRegisterRequest()
            {
                Id = register.Id,
                SupplierSystemId = client.SupplierSystemId,
            };

            var getRegisterReply = await client.Register.GetAsync(getRegisterRequest, client.AuthorizedMetadata);

            Assert.Equal(RegisterStatus.Open, getRegisterReply.Register.Status);
        }

        [Theory]
        [ClassData(typeof(RegisterOpenStatusCertificationKeys))]
        public async Task Should_have_register_status_closed(string certification)
        {
            var accountContext = new AccountContext
            {
                AccountName = _faker.Company.CompanyName(),
                Certification = certification,
                SupplierSystemId = _faker.Random.AlphaNumeric(10),
            };

            var systemId = _faker.Random.AlphaNumeric(10);

            var client = await _setup.CreateClient(accountContext);

            var createRegisterRequest = new CreateRegisterRequest()
            {
                SupplierSystemId = client.SupplierSystemId,
                SystemId = systemId
            };

            var register = await client.Register.CreateRegisterAsync(createRegisterRequest, client.AuthorizedMetadata);

            var closeRegisterRequest = new CloseRegisterRequest()
            {
                SupplierSystemId = client.SupplierSystemId,
                SystemId = systemId
            };

            await client.Register.CloseRegisterAsync(closeRegisterRequest, client.AuthorizedMetadata);

            var getRegisterRequestAfterClosing = new GetRegisterRequest()
            {
                Id = register.Id,
                SupplierSystemId = client.SupplierSystemId,
            };

            var getRegisterReplyAfterClosing = await client.Register.GetAsync(getRegisterRequestAfterClosing, client.AuthorizedMetadata);

            Assert.Equal(RegisterStatus.Closed, getRegisterReplyAfterClosing.Register.Status);
        }

        [Theory]
        [ClassData(typeof(RegisterOpenStatusCertificationKeys))]
        public async Task Should_return_all_registers(string certification)
        {
            var accountContext = new AccountContext
            {
                AccountName = _faker.Company.CompanyName(),
                Certification = certification,
                SupplierSystemId = _faker.Random.AlphaNumeric(10)
            };

            var client = await _setup.CreateClient(accountContext);

            var createRegisterRequest1 = new CreateRegisterRequest()
            {
                SupplierSystemId = client.SupplierSystemId,
            };

            var createRegisterReply1 = await client.Register.CreateRegisterAsync(createRegisterRequest1, client.AuthorizedMetadata);

            var createRegisterRequest2 = new CreateRegisterRequest()
            {
                SupplierSystemId = client.SupplierSystemId,
            };

            var createRegisterReply2 = await client.Register.CreateRegisterAsync(createRegisterRequest2, client.AuthorizedMetadata);

            var getAllRegistersRequest = new GetAllRegistersRequest()
            {
                SupplierSystemId = client.SupplierSystemId,
            };

            var getAllRegistersReply = await client.Register.GetAllAsync(getAllRegistersRequest, client.AuthorizedMetadata);

            Assert.Equal(2, getAllRegistersReply.Registers.Count);

            Assert.Contains(getAllRegistersReply.Registers, x => x.Id == createRegisterReply1.Id);

            Assert.Contains(getAllRegistersReply.Registers, x => x.Id == createRegisterReply2.Id);
        }

        [Theory]
        [ClassData(typeof(RegisterOpenStatusCertificationKeys))]
        public async Task Should_not_access_register_from_another_account(string certification)
        {
            var accountContext1 = new AccountContext
            {
                AccountName = _faker.Company.CompanyName(),
                Certification = certification,
                SupplierSystemId = _faker.Random.AlphaNumeric(10)
            };

            var accountContext2 = new AccountContext
            {
                AccountName = _faker.Company.CompanyName(1),
                Certification = certification,
                SupplierSystemId = _faker.Random.AlphaNumeric(10)
            };

            var client1 = await _setup.CreateClient(accountContext1);
            var client2 = await _setup.CreateClient(accountContext2);

            var createRegisterRequest1 = new CreateRegisterRequest()
            {
                SupplierSystemId = client1.SupplierSystemId,
            };

            var createRegisterReply1 = await client1.Register.CreateRegisterAsync(createRegisterRequest1, client1.AuthorizedMetadata);

            var createRegisterRequest2 = new CreateRegisterRequest()
            {
                SupplierSystemId = client2.SupplierSystemId,
            };

            var createRegisterReply2 = await client2.Register.CreateRegisterAsync(createRegisterRequest2, client2.AuthorizedMetadata);

            var getRegisterRequest1 = new GetRegisterRequest()
            {
                Id = createRegisterReply1.Id,
                SupplierSystemId = client2.SupplierSystemId,
            };

            var getRegisterReply1 = await client2.Register.GetAsync(getRegisterRequest1, client2.AuthorizedMetadata);

            var getRegisterRequest2 = new GetRegisterRequest()
            {
                Id = createRegisterReply2.Id,
                SupplierSystemId = client1.SupplierSystemId,
            };

            var getRegisterReply2 = await client1.Register.GetAsync(getRegisterRequest2, client1.AuthorizedMetadata);

            Assert.Null(getRegisterReply1.Register);
            Assert.Null(getRegisterReply2.Register);
        }
    }
}
