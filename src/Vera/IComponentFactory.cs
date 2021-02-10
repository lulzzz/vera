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
        
        // TODO(kevin): chop these up in to it's own factory to reduce scope?
        IInvoiceValidator CreateInvoiceValidator();
        IInvoiceBucketGenerator CreateInvoiceBucketGenerator();
        IInvoiceNumberGenerator CreateInvoiceNumberGenerator();
        IPackageSigner CreatePackageSigner();
        IThermalReceiptGenerator CreateThermalReceiptGenerator();

        IAuditWriter CreateAuditWriter();
    }
}