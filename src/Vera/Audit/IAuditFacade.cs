using System;
using System.Threading.Tasks;
using Vera.Stores;

namespace Vera.Audit
{
    public interface IAuditFacade<T>
    {
        Task Process(AuditContext context, AuditCriteria criteria);
    }

    public sealed class AuditFacade<T> : IAuditFacade<T>
    {
        private readonly IAuditFactory<T> _auditFactory;
        private readonly IInvoiceStore _invoiceStore;

        public AuditFacade(IAuditFactory<T> auditFactory, IInvoiceStore invoiceStore)
        {
            _auditFactory = auditFactory;
            _invoiceStore = invoiceStore;
        }

        public async Task Process(AuditContext context, AuditCriteria criteria)
        {
            // TODO(kevin): check if there is an archive already that mataches (or overlaps) the criteria
            
            // TODO(kevin): get all the data that is needed within the given criteria and generate the SAFT 2.0 models for input
            var audit = new StandardAuditFileTaxation.Audit(new StandardAuditFileTaxation.Header
            {
                CreationTime = DateTime.UtcNow,
                Country = context.Account.Address.Country,
                Region = context.Account.Address.Region,
                
                // TODO(kevin): what to put here?
                SoftwareCompanyName = string.Empty,
                SoftwareName = context.SoftwareName,
                SoftwareVersion = context.SoftwareVersion,

                // Version of the SAFT file
                Version = "2.0",
                SelectionCriteria = new StandardAuditFileTaxation.SelectionCriteria
                {
                    PeriodStart = criteria.StartFiscalPeriod,
                    PeriodStartYear = criteria.StartFiscalYear,
                    PeriodEnd = criteria.EndFiscalPeriod,
                    PeriodEndYear = criteria.EndFiscalYear,
                    SelectionStartDate = criteria.StartDate,
                    SelectionEndDate = criteria.EndDate,
                    // TODO(kevin): fill in missing properties      
                },
                Company = new StandardAuditFileTaxation.Company
                {
                    // TODO(kevin): map all the properties
                    SystemID = context.Account.Id.ToString(),
                    Name = context.Account.Name,
                    RegistrationNumber = context.Account.RegistrationNumber
                }
            });

            await foreach (var invoice in _invoiceStore.List(criteria))
            {
                // TODO(kevin): apply the invoice to the audit one by one and fill it up
                // audit.MasterFiles.Customers
                // audit.MasterFiles.Employees
                // audit.MasterFiles.Periods
                // audit.MasterFiles.Products
                // audit.MasterFiles.TaxTable

                // audit.SourceDocuments.Payments
                // audit.SourceDocuments.SalesInvoices

                audit.SourceDocuments.Payments.Add(new StandardAuditFileTaxation.Payment
                {
                    
                });

                // audit.SourceDocuments.SalesInvoices.Add(new StandardAuditFileTaxation.Invoice
                // {
                //     Number = invoice.Number,
                //     Date = invoice.Date,
                //     IsManual = invoice.Manual,
                //     Period = invoice.FiscalPeriod,
                //     PeriodYear = invoice.FiscalYear,
                //     TerminalID = invoice.TerminalId,
                //     SystemID = invoice.SystemId,
                //     Signature = invoice.Signature,
                //     RawSignature = invoice.RawSignature
                // });
            }

            var transformer = _auditFactory.CreateAuditTransformer();
            var result = transformer.Transform(context, criteria, audit);

            var archive = _auditFactory.CreateAuditArchive();
            await archive.Archive(criteria, result);
        }
    }
}