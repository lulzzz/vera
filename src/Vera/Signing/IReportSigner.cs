using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Signing
{
    public interface IReportSigner
    {
        Task<Signature> Sign(RegisterReport report, Signature previousSignature);
    }

    public class NoopPackageSigner : IReportSigner
    {
        public Task<Signature> Sign(RegisterReport report, Signature previousSignature) => null;
    }
}
