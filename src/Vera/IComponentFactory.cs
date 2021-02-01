using System.Threading.Tasks;
using Vera.Audits;
using Vera.Concurrency;
using Vera.Configuration;
using Vera.Invoices;
using Vera.Signing;
using Vera.Thermal;

namespace Vera
{
    // TODO(kevin): add docs
    public interface IComponentFactory
    {
        ILocker CreateLocker();
        IInvoiceBucketGenerator CreateInvoiceBucketGenerator();
        IInvoiceNumberGenerator CreateInvoiceNumberGenerator();
        IPackageSigner CreatePackageSigner();
        IThermalReceiptGenerator CreateThermalReceiptGenerator();
        IAuditWriter CreateAuditWriter();
    }
}