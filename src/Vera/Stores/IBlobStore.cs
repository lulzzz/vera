using System;
using System.IO;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Stores
{
    public interface IBlobStore
    {
        Task<string> Store(Guid accountId, Blob blob);
        public Task<Blob?> Read(Guid accountId, string name);
    }
}