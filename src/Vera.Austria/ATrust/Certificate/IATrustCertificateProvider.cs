using System.Threading.Tasks;

namespace Vera.Austria.ATrust.Certificate
{
  public interface IATrustCertificateProvider
  {
    Task<string> Provide(string host, string username);
  }
}