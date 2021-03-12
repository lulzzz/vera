using System.Threading.Tasks;

namespace Vera.Austria.ATrust.Session
{
  /// <summary>
  /// Caches the sessions so that no requests to the API of ATrust are required to login.
  /// </summary>
  public interface IATrustSessionPool
  {
    /// <summary>
    /// Attempts to grab a (new) session for the given parameters.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    ValueTask<IATrustSession> Acquire(string host, string username, string password);

    /// <summary>
    /// Release the resources of the session.
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    ValueTask Release(IATrustSession session);
  }
}