using System;
using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Signing
{
    public interface IReportSigner
    {
        Task<Signature> Sign(RegisterReport report, Signature previousSignature);
    }

    public class NullReportSigner : IReportSigner
    {
        public Task<Signature> Sign(RegisterReport report, Signature previousSignature) =>
            Task.FromResult(new Signature
            {
                Input = string.Empty,
                Output = Array.Empty<byte>()
            });
    }
}
