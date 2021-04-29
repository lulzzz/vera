using System.Threading.Tasks;
using Bogus;
using Grpc.Net.Client;
using Vera.Grpc;
using Xunit;

namespace Vera.Integration.Tests.Common
{
    public class TokenServiceTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly Setup _setup;

        public TokenServiceTests(ApiWebApplicationFactory fixture)
        {
            _setup = fixture.CreateSetup();
        }

        [Fact]
        public async Task Should_be_able_to_request_token()
        {
            var login = await _setup.CreateLogin();

            var tokenRequest = new TokenRequest
            {
                Username = new Faker().Internet.UserName()
            };

            var result =
                await _setup.TokenClient.GenerateAsync(tokenRequest, _setup.CreateAuthorizedMetadata(login.Token));

            Assert.NotNull(result.Token);
            Assert.NotNull(result.RefreshToken);
        }
    }
}