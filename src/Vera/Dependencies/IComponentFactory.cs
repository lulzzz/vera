using Vera.Audits;
using Vera.Configuration;
using Vera.Invoices;
using Vera.Thermal;

namespace Vera.Dependencies
{
    /// <summary>
    /// Responsible for creating the components that can be overriden per certification implementation.
    /// </summary>
    public interface IComponentFactory : IInvoiceComponentFactory 
    {
        IConfigurationValidator CreateConfigurationValidator();
        IThermalReceiptGenerator CreateThermalReceiptGenerator();
        IAuditWriter CreateAuditWriter();
    }
}