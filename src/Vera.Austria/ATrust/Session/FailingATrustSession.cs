using System;
using System.Threading.Tasks;

namespace Vera.Austria.ATrust.Session
{
  /// <summary>
  /// Will always fail no matter what happens. This is only used for testing purposes to test the scenario where ATrust would
  /// be offline or other errors are thrown from the API.
  /// </summary>
  public sealed class FailingATrustSession : IATrustSession
  {
    public Task<string> Sign(byte[] toSign)
    {
      throw new Exception("Will always fail");
    }

    public Task<string> SignJWS(string payload)
    {
      throw new Exception("Will always fail");
    }

    public string Username { get; set; }
    public string ID { get; set; }
    public string Key { get; set; }
  }
}