using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Signing
{
    public interface IPackageSigner
    {
        Task<Signature> Sign(Package package);
    }
}