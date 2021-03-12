using System.Threading.Tasks;

namespace Vera.Austria.ATrust.Session
{
  public interface IATrustSession
  {
    /// <summary>
    /// Requests the ATrust API to sign the given data.
    /// </summary>
    /// <param name="toSign">Data that should be signed.</param>
    /// <returns>Given data signed as a Base64 string.</returns>
    Task<string> Sign(byte[] toSign);

    /// <summary>
    /// Requests the ATrust API to sign the given payload and return a JWS.
    /// </summary>
    /// <param name="payload">The payload to be signed.</param>
    /// <returns>A JWS.</returns>
    Task<string> SignJWS(string payload);

    /// <summary>
    /// Username that is associated with the session. A session is created with a combination
    /// of username/password.
    /// </summary>
    string Username { get; }

    /// <summary>
    /// Unique ID for this signing session.
    /// </summary>
    string ID { get; }

    /// <summary>
    /// Unique key for this signing session.
    /// </summary>
    string Key { get; }
  }
}