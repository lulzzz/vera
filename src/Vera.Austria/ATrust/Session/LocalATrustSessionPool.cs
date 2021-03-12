using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Vera.Austria.ATrust.Session
{
  public sealed class LocalATrustSessionPool : IATrustSessionPool
  {
    private readonly IATrustSessionFactory _sessionFactory;
    private readonly MemoryCache _cache;

    public LocalATrustSessionPool(IATrustSessionFactory sessionFactory)
    {
      _sessionFactory = sessionFactory;
      _cache = new MemoryCache(new MemoryCacheOptions
      {
        SizeLimit = 4
      });
    }

    public async ValueTask<IATrustSession> Acquire(string host, string username, string password)
    {
      var session = await _cache.GetOrCreateAsync(username, entry =>
      {
        // Sessions expire after 30 minutes of inactivity
        entry.SlidingExpiration = TimeSpan.FromMinutes(30);
        entry.Size = 1;
        
        return _sessionFactory.Create(host, username, password);
      });
      
      return new PooledATrustSession(this, session, host, password);
    }

    public ValueTask Release(IATrustSession session)
    {
      _cache.Remove(session.Username);

      return ValueTask.CompletedTask;
    }
  }
}