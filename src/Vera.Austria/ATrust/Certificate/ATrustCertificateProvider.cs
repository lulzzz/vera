using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Vera.Austria.ATrust.Models;

namespace Vera.Austria.ATrust.Certificate
{
  public sealed class ATrustCertificateProvider : IATrustCertificateProvider
  {
    private readonly IHttpClientFactory _httpClientFactory;

    public ATrustCertificateProvider(IHttpClientFactory httpClientFactory)
    {
      _httpClientFactory = httpClientFactory;
    }

    public async Task<string> Provide(string host, string username)
    {
      var response = await _httpClientFactory
        .CreateClient()
        .GetFromJsonAsync<ATrustCertificateResponse>($"{host}/{username}/Certificate");
      
      return response.CertificateSerialNumberHex;
    }
  }
}