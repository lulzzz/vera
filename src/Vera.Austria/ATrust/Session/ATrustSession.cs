using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Vera.Austria.ATrust.Models;

namespace Vera.Austria.ATrust.Session
{
  public sealed class ATrustSession : IATrustSession
  {
    private readonly HttpClient _client;

    public ATrustSession(HttpClient client, string username, string sessionID, string sessionKey)
    {
      _client = client;

      Username = username;
      ID = sessionID;
      Key = sessionKey;
    }

    public async Task<string> Sign(byte[] toSign)
    {
      try
      {
        var response = await _client.PostAsJsonAsync($"Session/{ID}/Sign", new
        {
          sessionkey = Key,
          to_be_signed = Convert.ToBase64String(toSign)
        });
        
        response.EnsureSuccessStatusCode();
        
        var result = JsonConvert.DeserializeObject<ATrustSignatureResponse>(await response.Content.ReadAsStringAsync());

        if (string.IsNullOrWhiteSpace(result.Signature))
        {
          throw new ApplicationException("Did not get back a signature");
        }

        return result.Signature;
      }
      catch (HttpRequestException e)
      {
        if (e.StatusCode == HttpStatusCode.Unauthorized)
        {
          throw new ATrustSessionExpiredException(this);
        }

        throw;
      }
    }

    public async Task<string> SignJWS(string payload)
    {
      try
      {
        var response = await _client.PostAsJsonAsync($"Session/{ID}/Sign/JWS", new
        {
          sessionkey = Key,
          jws_payload = payload
        });

        response.EnsureSuccessStatusCode();
        
        var result = JsonConvert.DeserializeObject<ATrustJWSResponse>(await response.Content.ReadAsStringAsync());

        if (string.IsNullOrWhiteSpace(result.Result))
        {
          throw new ApplicationException("Did not get back a signature");
        }

        return result.Result;
      }
      catch (HttpRequestException e)
      {
        if (e.StatusCode == HttpStatusCode.Unauthorized)
        {
          throw new ATrustSessionExpiredException(this);
        }

        throw;
      }
    }

    public string Username { get; }
    public string ID { get; }
    public string Key { get; }
  }
}