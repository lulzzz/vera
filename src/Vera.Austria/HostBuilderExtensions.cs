using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vera.Austria.ATrust.Certificate;
using Vera.Austria.ATrust.Session;

namespace Vera.Austria
{
  public static class HostBuilderExtensions
  {
    public static IHostBuilder UseVeraAustria(this IHostBuilder builder)
    {
      return builder.ConfigureServices((context, collection) =>
      {
        // TODO(kevin): cluster implementations for sessions and certificates?
        
        collection.AddSingleton<IATrustSessionFactory, ATrustSessionFactory>();
        collection.AddSingleton<IATrustSessionPool, LocalATrustSessionPool>();
        collection.AddSingleton<IATrustCertificateProvider>(x => 
          new LocalATrustCertificateProvider(
            new ATrustCertificateProvider(x.GetRequiredService<IHttpClientFactory>())));
        
        
      });
    }
  }
}