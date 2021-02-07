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

            var registerRequest = new RegisterRequest
            {
                Username = faker.Internet.UserName(),
                Password = faker.Internet.Password(),
                CompanyName = faker.Company.CompanyName()
            };

            var registerClient = new RegisterService.RegisterServiceClient(_channel);
            await registerClient.RegisterAsync(registerRequest);

            var loginClient = new LoginService.LoginServiceClient(_channel);
            var loginResult = await loginClient.LoginAsync(new LoginRequest
            {
                Username = registerRequest.Username,
                Password = registerRequest.Password,
                CompanyName = registerRequest.CompanyName
            });

            Assert.NotNull(loginResult.Token);
            Assert.NotNull(loginResult.RefreshToken);
        }
    }
}