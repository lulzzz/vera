using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using Vera.WebApi.Models;
using Xunit;

namespace Vera.Integration.Tests
{
    public class RegistrationTests : IClassFixture<ApiWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public RegistrationTests(ApiWebApplicationFactory fixture)
        {
            _client = fixture.CreateClient();
        }

        [Fact]
        public async Task Should_be_able_to_register_and_login()
        {
            var faker = new Faker();

            var registerRequest = new Register
            {
                Username = faker.Internet.UserName(),
                Password = faker.Internet.Password(),
                CompanyName = faker.Company.CompanyName()
            };

            using var registerResponse = await _client.PostAsJsonAsync("/register", registerRequest);

            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

            var loginRequest = new Login
            {
                Username = registerRequest.Username,
                Password = registerRequest.Password,
                CompanyName = registerRequest.CompanyName
            };

            using var loginResponse = await _client.PostAsJsonAsync("/login", loginRequest);

            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var loginObject = await loginResponse.Content.ReadAsAsync<LoginResponse>();

            Assert.NotNull(loginObject.Token);
            Assert.NotNull(loginObject.RefreshToken);
        }
    }
}