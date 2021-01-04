using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Grpc.Core;
using Grpc.Net.Client;
using Vera.Grpc;
using Vera.Grpc.Shared;
using Xunit;

namespace Vera.Integration.Tests.Common
{
    public class AccountTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly GrpcChannel _channel;

        public AccountTests(ApiWebApplicationFactory fixture)
        {
            var client = fixture.CreateClient();

            _channel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions
            {
                HttpClient = client
            });
        }

        [Fact]
        public async Task Should_be_able_to_create_accounts()
        {
            var faker = new Faker();

            var loginObject = await GetToken(faker);

            var metadata = new Metadata
            {
                {"authorization", $"bearer {loginObject.Token}"}
            };

            var accountToCreate = new CreateAccountRequest
            {
                Name = faker.Company.CompanyName(),
                Certification = "LALA"
            };

            var accountClient = new AccountService.AccountServiceClient(_channel);
            await accountClient.CreateAsync(accountToCreate, metadata);

            using var listCall = accountClient.ListAsync(new Empty(), metadata);
            var result = await listCall.ResponseAsync;
            var accounts = result.Accounts;

            Assert.NotNull(accounts);
            Assert.Single(accounts);

            var account = accounts.First();

            Assert.Equal(accountToCreate.Name, account.Name);
            Assert.Equal(accountToCreate.Certification, account.Certification);
        }

        private async Task<TokenReply> GetToken(Faker faker)
        {
            var registerRequest = new RegisterRequest
            {
                Username = faker.Internet.UserName(),
                Password = faker.Internet.Password(),
                CompanyName = faker.Company.CompanyName()
            };

            var registerClient = new RegisterService.RegisterServiceClient(_channel);
            using var registerCall = registerClient.RegisterAsync(registerRequest);

            var registerResult = await registerCall.ResponseAsync;

            var loginClient = new LoginService.LoginServiceClient(_channel);
            using var loginCall = loginClient.LoginAsync(new LoginRequest
            {
                Username = registerRequest.Username,
                Password = registerRequest.Password,
                CompanyName = registerRequest.CompanyName
            });

            return await loginCall.ResponseAsync;
        }
    }
}