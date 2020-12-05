using System.Threading.Tasks;
using Bogus;
using Grpc.Net.Client;
using Vera.Grpc;
using Xunit;
using LoginService = Vera.Grpc.LoginService;
using RegisterService = Vera.Grpc.RegisterService;

namespace Vera.Integration.Tests.Common
{
    public class RegistrationTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly GrpcChannel _channel;

        public RegistrationTests(ApiWebApplicationFactory fixture)
        {
            var client = fixture.CreateClient();

            _channel = GrpcChannel.ForAddress(client.BaseAddress, new GrpcChannelOptions
            {
                HttpClient = client
            });
        }

        [Fact]
        public async Task Should_be_able_to_register_and_login()
        {
            var faker = new Faker();

            var registerRequest = new Grpc.RegisterRequest
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

            var loginResult = await loginCall.ResponseAsync;

            Assert.NotNull(loginResult.Token);
            Assert.NotNull(loginResult.RefreshToken);
        }
    }
}