using Vera.Audits;
using Vera.Configuration;
using Vera.Invoices;
using Vera.Registers;
using Vera.Reports;
using Vera.Thermal;

namespace Vera.Dependencies
{
    /// <summary>
    /// Responsible for creating the components that can be overriden per certification implementation.
    /// </summary>
    public interface IComponentFactory : IInvoiceComponentFactory, IReportComponentFactory
    {
        IConfigurationValidator CreateConfigurationValidator();
        IThermalReceiptGenerator CreateThermalReceiptGenerator();
        IReportReceiptGenerator CreateThermalReportGenerator();

        IAuditWriter CreateAuditWriter();

        IRegisterInitializer CreateRegisterInitializer();

        IRegisterCloser CreateRegisterCloser();
    }
}