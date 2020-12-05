using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Grpc.Core;
using Grpc.Net.Client;
using Vera.Grpc;

namespace Vera.Integration.Tests
{
    public class Setup
    {
        private readonly Faker _faker;

        public Setup(ChannelBase channel, Faker faker)
        {
            _faker = faker;

            RegisterClient = new RegisterService.RegisterServiceClient(channel);
            LoginClient = new LoginService.LoginServiceClient(channel);
            AccountClient = new AccountService.AccountServiceClient(channel);
        }

        public async Task<string> CreateLogin()
        {
            var registerRequest = new RegisterRequest
            {
                Username = _faker.Internet.UserName(),
                Password = _faker.Internet.Password(),
                CompanyName = _faker.Company.CompanyName()
            };

            using var registerCall = RegisterClient.RegisterAsync(registerRequest);

            await registerCall.ResponseAsync;

            using var loginCall = LoginClient.LoginAsync(new LoginRequest
            {
                Username = registerRequest.Username,
                Password = registerRequest.Password,
                CompanyName = registerRequest.CompanyName
            });

            var loginResponse = await loginCall.ResponseAsync;

            Token = loginResponse.Token;

            return loginResponse.Token;
        }

        public async Task<string> CreateAccount(string token)
        {
            var accountToCreate = new CreateAccountRequest
            {
                Name = _faker.Company.CompanyName(),
                Certification = "PT"
            };

            using var createAccountCall = AccountClient.CreateAsync(accountToCreate, CreateAuthorizedMetadata());
            var createAccountReply = await createAccountCall.ResponseAsync;

            return createAccountReply.Id;
        }

        public Metadata CreateAuthorizedMetadata()
        {
            return new Metadata
            {
                {"authorization", $"bearer {Token}"}
            };
        }

        public string Token { get; set; }

        public RegisterService.RegisterServiceClient RegisterClient { get; set; }
        public LoginService.LoginServiceClient LoginClient { get; set; }
        public AccountService.AccountServiceClient AccountClient { get; set; }
    }
}