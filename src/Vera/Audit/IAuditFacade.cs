using System;
using System.Threading.Tasks;
using Vera.Audit.Extract;
using Vera.Models;
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

            var extractors = new IAuditDataExtractor[]
            {
                new CustomerAuditDataExtractor(),
                new EmployeeAuditDataExtractor(),
                new PaymentAuditDataExtractor(),
                new ProductAuditDataExtractor(),
                new TaxTableAuditExtractor(), 
                new InvoiceAuditDataExtractor(),
                new PaymentAuditDataExtractor()
            };
            
            // Extract data from all the invoices
            await foreach (var invoice in _invoiceStore.List(criteria))
            {
                foreach (var e in extractors)
                {
                    e.Extract(invoice);
                }
            }
            
            // Apply the extracted data to the auditing model
            foreach (var e in extractors)
            {
                e.Apply(audit);
            }
            
            var transformer = _auditFactory.CreateAuditTransformer();
            var result = transformer.Transform(context, criteria, audit);

            var archive = _auditFactory.CreateAuditArchive();
            await archive.Archive(criteria, result);
        }
    }
}