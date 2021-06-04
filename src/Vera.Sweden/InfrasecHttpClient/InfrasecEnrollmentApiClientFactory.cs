using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Vera.Sweden.InfrasecHttpClient.Contracts;
using Vera.Sweden.Models.Configs;
using Vera.Sweden.Utils;

namespace Vera.Sweden.InfrasecHttpClient
{
  public class InfrasecEnrollmentApiClientFactory : IInfrasecEnrollmentApiClientFactory
  {
    private readonly IHttpClientFactory _httpClientFactory;
    private const string InfrasecEnrollmentClientName = "Infrasec_Enrollment_Http_Client";

    public InfrasecEnrollmentApiClientFactory(IHttpClientFactory httpClientFactory)
    {
      _httpClientFactory = httpClientFactory;
    }

    public IInfrasecClient Create(SwedenConfigs swedenConfigs)
    {
      // .pem ServerTrust_Verify file
      var infrasecEnrollmentServerTrustBytes = swedenConfigs.InfrasecEnrollmentCertServerTrustPem;

      var rootCertificate = new X509Certificate2(infrasecEnrollmentServerTrustBytes);
      var rootCertificateCollection = new X509Certificate2Collection(rootCertificate);

      // .pfx enrollment file
      var infrasecEnrollmentCertificateBytes = swedenConfigs.InfrasecReceiptCertPfx;
      var infrasecEnrollmentCertKey = swedenConfigs.InfrasecEnrollmentCertPfxKey;

      var clientHandler = new HttpClientHandler
      {
        ServerCertificateCustomValidationCallback = InfrasecCertificateValidator.CreateCustomRootValidator(rootCertificateCollection)
      };
      clientHandler.ClientCertificates.Add(new X509Certificate2(infrasecEnrollmentCertificateBytes, infrasecEnrollmentCertKey));
      
      // make sure we are using a unique client
      var httpClient = _httpClientFactory.CreateClient(InfrasecEnrollmentClientName);
      httpClient.BaseAddress = new Uri(swedenConfigs.InfrasecEnrollmentApiUrl);
      
      return new InfrasecClient(httpClient);
    }
  }
}