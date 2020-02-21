using System.Threading.Tasks;

namespace Vera.Signing
{
    public interface IPackageSigner
    {
        Task<PackageSignResult> Sign(Package package);
    }
}