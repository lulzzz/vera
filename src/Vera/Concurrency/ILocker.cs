using System;
using System.Threading.Tasks;

namespace Vera.Concurrency
{
    public interface ILocker
    {
        Task<IAsyncDisposable> Lock(string resource, TimeSpan timeout);
    }
}