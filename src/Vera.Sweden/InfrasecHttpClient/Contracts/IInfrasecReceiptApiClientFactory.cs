using System.Threading.Tasks;

namespace Vera.Sweden.InfrasecHttpClient.Contracts
{
  public interface IInfrasecReceiptApiClientFactory
  {
    Task<IInfrasecClient> Create(int organizationUnitID);
  }
}