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
    }
}
