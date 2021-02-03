using System.Threading.Tasks;
using Vera.Audits;
using Vera.Concurrency;
using Vera.Configuration;
using Vera.Invoices;
using Vera.Signing;
using Vera.Thermal;

namespace Vera
{
    /// <summary>
    /// Responsible for creating the components that can be overriden per certification implementation.
    /// </summary>
    public interface IComponentFactory
    {
        IConfigurationValidator CreateConfigurationValidator();
        IInvoiceBucketGenerator CreateInvoiceBucketGenerator();
        IInvoiceNumberGenerator CreateInvoiceNumberGenerator();
        IPackageSigner CreatePackageSigner();
        IThermalReceiptGenerator CreateThermalReceiptGenerator();
        IAuditWriter CreateAuditWriter();
    }
}