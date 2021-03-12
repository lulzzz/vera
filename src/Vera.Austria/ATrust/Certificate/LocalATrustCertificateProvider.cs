using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Vera.Austria.ATrust.Certificate
{
    public class LocalATrustCertificateProvider : IATrustCertificateProvider
    {
        private readonly IATrustCertificateProvider _provider;
        private readonly ConcurrentDictionary<string, string> _cache;

        public LocalATrustCertificateProvider(IATrustCertificateProvider provider)
        {
            _provider = provider;
            _cache = new ConcurrentDictionary<string, string>();
        }

        public async Task<string> Provide(string host, string username)
        {
            var key = $"{host}{username}";

            if (!_cache.TryGetValue(key, out var result))
            {
                _cache[key] = result = await _provider.Provide(host, username);
            }

            return result;
        }
    }
}