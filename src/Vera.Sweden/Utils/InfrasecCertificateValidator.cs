using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Vera.Sweden.Utils
{
  /// <summary>
  /// https://www.meziantou.net/custom-certificate-validation-in-dotnet.htm#dotnet-5-way-of-vali
  /// </summary>
  public static class InfrasecCertificateValidator
  {
    public static RemoteCertificateValidationCallback CreateCustomRootRemoteValidator(
      X509Certificate2Collection trustedRoots)
    {
      if (trustedRoots == null)
        throw new ArgumentNullException(nameof(trustedRoots));
      if (trustedRoots.Count == 0)
        throw new ArgumentException("No trusted roots were provided", nameof(trustedRoots));

      // Let's avoid complex state and/or race conditions by making copies of these collections.
      // Then the delegates should be safe for parallel invocation (provided they are given distinct inputs, which they are).
      var roots = new X509Certificate2Collection(trustedRoots);

      return (sender, serverCert, chain, errors) =>
      {
        // Missing cert or the destination hostname wasn't valid for the cert.
        if ((errors & ~SslPolicyErrors.RemoteCertificateChainErrors) != 0)
        {
          return false;
        }

        for (var i = 1; i < chain.ChainElements.Count; i++)
        {
          chain.ChainPolicy.ExtraStore.Add(chain.ChainElements[i].Certificate);
        }

        chain.ChainPolicy.CustomTrustStore.Clear();
        chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        chain.ChainPolicy.CustomTrustStore.AddRange(roots);
        return chain.Build((X509Certificate2) serverCert);
      };
    }

    public static Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>
      CreateCustomRootValidator(X509Certificate2Collection trustedRoots)
    {
      var callback = CreateCustomRootRemoteValidator(trustedRoots);
      return (message, serverCert, chain, errors) => callback(null, serverCert, chain, errors);
    }
  }
}