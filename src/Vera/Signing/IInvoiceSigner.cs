using System.Threading.Tasks;
using Vera.Models;

namespace Vera.Signing
{
    public interface IInvoiceSigner
    {
        Task<Signature> Sign(Invoice invoice, Signature previousSignature);
    }
}