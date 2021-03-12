using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Vera.Austria.ATrust.Models;

namespace Vera.Austria.ATrust.Session
{
  public sealed class ATrustSessionFactory : IATrustSessionFactory
  {
    private readonly IHttpClientFactory _httpClientFactory;

    public ATrustSessionFactory(IHttpClientFactory httpClientFactory)
    {
      _httpClientFactory = httpClientFactory;
    }

    public async Task<IATrustSession> Create(string host, string username, string password)
    {
      var client = _httpClientFactory.CreateClient();
      var session = await StartSession(client, username, password);

      return new ATrustSession(client, username, session.SessionID, session.SessionKey);
    }

    public Task<IATrustSession> CreateFromSession(string host, string username, string sessionID, string sessionKey)
    {
      var client = _httpClientFactory.CreateClient();

      return Task.FromResult<IATrustSession>(new ATrustSession(client, username, sessionID, sessionKey));
    }

    private static async Task<ATrustSessionResponse> StartSession(HttpClient client, string username, string password)
    {
      // PUT Session/{username}
      // See 2.4.1 of the docs
      var response = await client.PutAsJsonAsync($"Session/{username}", new
      {
        password
      });

      response.EnsureSuccessStatusCode();

      return JsonConvert.DeserializeObject<ATrustSessionResponse>(await response.Content.ReadAsStringAsync());
    }
  }
}