using System.Threading.Tasks;

namespace Vera.Austria.ATrust.Session
{
  public interface IATrustSessionFactory
  {
    Task<IATrustSession> Create(string host, string username, string password);
    Task<IATrustSession> CreateFromSession(string host, string username, string sessionID, string sessionKey);
  }
}