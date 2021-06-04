using System.Net.Http;
using System.Threading.Tasks;

namespace Vera.Sweden.InfrasecHttpClient.Contracts
{
  public interface IInfrasecClient
  {
    Task<HttpResponseMessage> SendInfrasecEnrollmentRequest<T>(T requestBody);
    Task<HttpResponseMessage> SendInfrasecReceiptRequest<T>(T requestBody);
  }
}