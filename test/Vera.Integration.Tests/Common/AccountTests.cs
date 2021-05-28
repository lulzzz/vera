using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Vera.Grpc;
using Vera.Grpc.Shared;
using Xunit;

namespace Vera.Integration.Tests.Common
{
    public class AccountTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Setup _setup;

        public AccountTests(ApiWebApplicationFactory fixture)
        {
            _setup = fixture.CreateSetup();
        }

        [Fact]
        public async Task Should_be_able_to_create_accounts()
        {
            var faker = new Faker();

            var accountContext = new AccountContext
            {
                AccountName = faker.Company.CompanyName(),
                Certification = "LALA"
            };
            
            var client = await _setup.CreateClient(accountContext);
            
            using var listCall = client.Account.ListAsync(new Empty(), client.AuthorizedMetadata);
            var result = await listCall.ResponseAsync;
            var accounts = result.Accounts;

            Assert.NotNull(accounts);
            Assert.Single(accounts);

            var account = accounts.First();

            Assert.Equal(accountContext.AccountName, account.Name);
        }

        [Fact]
        public async Task Should_be_able_to_update_account()
        {
            var accountContext = new AccountContext
            {
                AccountName = "test-update-account",
                Certification = "LALA"
            };

            var client = await _setup.CreateClient(accountContext);
            
            var newName = new Faker().Company.CompanyName();

            await _setup.AccountClient.UpdateAsync(new UpdateAccountRequest
            {
                Id = client.AccountId,
                Name =  newName
            }, client.AuthorizedMetadata);

            var getAccountReply = await _setup.AccountClient.GetAsync(new GetAccountRequest
            {
                Id = client.AccountId
            }, client.AuthorizedMetadata);

            Assert.Equal(newName, getAccountReply.Name);
        }
    }
}