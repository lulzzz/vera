using System;
using System.IO;
using System.Threading.Tasks;

namespace Vera.Stores
{
    public interface IBlobStore
    {
        Task<string> Store(Guid accountId, Stream data);
    }
}