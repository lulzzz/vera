using System.Threading.Tasks;
using Bogus;
using Grpc.Net.Client;
using Vera.Grpc;
using Xunit;
using LoginService = Vera.Grpc.LoginService;
using UserRegisterService = Vera.Grpc.UserRegisterService;

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

            var registerRequest = new RegisterUserRequest
            {
                Username = faker.Internet.UserName(),
                Password = faker.Internet.Password(),
                CompanyName = faker.Company.CompanyName()
            };

            var registerClient = new UserRegisterService.UserRegisterServiceClient(_channel);
            await registerClient.RegisterUserAsync(registerRequest);

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