using System.Threading.Tasks;
using Bogus;
using Grpc.Net.Client;
using Vera.Grpc;
using Xunit;

namespace Vera.Integration.Tests.Common
{
    public class TokenServiceTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly GrpcChannel _channel;
        private readonly Faker _faker;

        public TokenServiceTests(ApiWebApplicationFactory fixture)
        {
            var client = fixture.CreateClient();

            _channel = GrpcChannel.ForAddress(client.BaseAddress!, new GrpcChannelOptions
            {
                HttpClient = client
            });

            _faker = new Faker();
        }

        [Fact]
        public async Task Should_be_able_to_request_token()
        {
            var setup = new Setup(_channel, _faker);

            await setup.CreateLogin();

            var account = await setup.CreateAccount();

            var tokenRequest = new TokenRequest
            {
                Username = _faker.Internet.UserName()
            };

            var tokenService = new TokenService.TokenServiceClient(_channel);
            using var tokenReply = tokenService.GenerateAsync(tokenRequest, setup.CreateAuthorizedMetadata());

            var result = await tokenReply.ResponseAsync;

            Assert.NotNull(result.Token);
            Assert.NotNull(result.RefreshToken);
        }
    }
}