using System;
using System.Threading.Tasks;
using Polly;

namespace Vera.Austria.ATrust.Session
{
  public sealed class PooledATrustSession : IATrustSession
  {
    private readonly IATrustSessionPool _pool;
    private IATrustSession _session;

    private readonly string _host;
    private readonly string _password;

    // TODO(kevin): not sure if I like passing around the password like this
    public PooledATrustSession(IATrustSessionPool pool, IATrustSession session, string host, string password)
    {
      _pool = pool;
      _session = session;
      _host = host;
      _password = password;
    }

    public Task<string> Sign(byte[] toSign)
    {
      return RetrySign(() => _session.Sign(toSign));
    }

    public Task<string> SignJWS(string payload)
    {
      return RetrySign(() => _session.SignJWS(payload));
    }

    private async Task<string> RetrySign(Func<Task<string>> sign)
    {
      try
      {
        var result = await Policy
          .Handle<ATrustSessionExpiredException>()
          .WaitAndRetryAsync(
            5, // TODO(kevin): is this a sane number of retries?
            (retryNumber) => TimeSpan.FromSeconds(retryNumber),
            async (exception, span) =>
            {
              // Session has expired, release it from the pool
              await _pool.Release(this);

              // Grab a new session to retry with in case of authentication failure
              _session = await _pool.Acquire(_host, _session.Username, _password);
            }
          )
          .ExecuteAndCaptureAsync(sign);

        if (result.Outcome == OutcomeType.Failure)
        {
          // Still failed after all the retries, throw up
          throw result.FinalException;
        }

        return result.Result;
      }
      catch (Exception)
      {
        // Any exception probably means that the session is no longer valid
        await _pool.Release(this);

        throw;
      }
    }

    public string Username => _session.Username;

    public string ID => _session.ID;

    public string Key => _session.Key;
  }
}