using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Grpc.Net.Client;
using Vera.Grpc;
using Vera.Grpc.Shared;
using Xunit;

namespace Vera.Integration.Tests.Common
{
    public class AccountTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly GrpcChannel _channel;
        private readonly Faker _faker;
        private readonly Setup _setup;

        public AccountTests(ApiWebApplicationFactory fixture)
        {
            var client = fixture.CreateClient();

            _channel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions
            {
                HttpClient = client
            });

            _faker = new Faker();
            _setup = new Setup(_channel, _faker);
        }

        [Fact]
        public async Task Should_be_able_to_create_accounts()
        {
            await _setup.CreateLogin();

            var metadata = _setup.CreateAuthorizedMetadata();

            var accountToCreate = new CreateAccountRequest
            {
                Name = _faker.Company.CompanyName(),
                Certification = "LALA"
            };

            await _setup.AccountClient.CreateAsync(accountToCreate, metadata);

            using var listCall = _setup.AccountClient.ListAsync(new Empty(), metadata);
            var result = await listCall.ResponseAsync;
            var accounts = result.Accounts;

            Assert.NotNull(accounts);
            Assert.Single(accounts);

            var account = accounts.First();

            Assert.Equal(accountToCreate.Name, account.Name);
        }

        [Fact]
        public async Task Should_be_able_to_update_account()
        {
            await _setup.CreateLogin();

            var metadata = _setup.CreateAuthorizedMetadata();

            var accountToCreate = new CreateAccountRequest
            {
                Name = _faker.Company.CompanyName(),
                Certification = "LALA"
            };

            var createAccountReply = await _setup.AccountClient.CreateAsync(accountToCreate, metadata);

            var newName = _faker.Company.CompanyName();

            await _setup.AccountClient.UpdateAsync(new UpdateAccountRequest
            {
                Id = createAccountReply.Id,
                Name =  newName
            }, metadata);

            var getAccountReply = await _setup.AccountClient.GetAsync(new GetAccountRequest
            {
                Id = createAccountReply.Id
            }, metadata);

            Assert.Equal(newName, getAccountReply.Name);
        }
    }
}