using System;
using System.Threading.Tasks;
using Vera.Concurrency;

namespace Vera.Austria.ATrust.Session
{
  public sealed class LockedATrustSessionPool : IATrustSessionPool
  {
    private readonly IATrustSessionPool _pool;
    private readonly ILocker _locker;

    public LockedATrustSessionPool(IATrustSessionPool pool, ILocker locker)
    {
      _pool = pool;
      _locker = locker;
    }

    public async ValueTask<IATrustSession> Acquire(string host, string username, string password)
    {
      await using (await _locker.Lock(username, TimeSpan.FromSeconds(15)))
      {
        return await _pool.Acquire(host, username, password);
      }
    }

    public async ValueTask Release(IATrustSession session)
    {
      await using (await _locker.Lock(session.Username, TimeSpan.FromSeconds(15)))
      {
        await _pool.Release(session);
      }
    }
  }
}