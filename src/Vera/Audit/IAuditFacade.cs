using System;
using System.Threading.Tasks;
using Vera.Audit.Extract;
using Vera.StandardAuditFileTaxation;
using Vera.Stores;
using Address = Vera.StandardAuditFileTaxation.Address;
using Company = Vera.StandardAuditFileTaxation.Company;

namespace Vera.Audit
{
    public interface IAuditFacade
    {
        Task Process(AuditContext context, AuditCriteria criteria);
    }

    public sealed class AuditFacade<T> : IAuditFacade
    {
        private readonly IAuditTransformer<T> _transformer;
        private readonly IAuditArchive<T> _archive;
        private readonly IInvoiceStore _invoiceStore;

        public AuditFacade(
            IAuditTransformer<T> transformer, 
            IAuditArchive<T> archive, 
            IInvoiceStore invoiceStore
        )
        {
            _transformer = transformer;
            _archive = archive;
            _invoiceStore = invoiceStore;
        }

        public async Task Process(AuditContext context, AuditCriteria criteria)
        {
            // TODO(kevin): check if there is an archive already that matches (or overlaps) the criteria

            var audit = await CreateAudit(context, criteria);
            var result = _transformer.Transform(context, criteria, audit);

            await _archive.Archive(criteria, result);
        }

        private async Task<StandardAuditFileTaxation.Audit> CreateAudit(AuditContext context, AuditCriteria criteria)
        {
            var audit = new StandardAuditFileTaxation.Audit(CreateHeader(context, criteria));
            
            // TODO(kevin): move to factory?
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

            return audit;
        }

        private static Header CreateHeader(AuditContext context, AuditCriteria criteria)
        {
            var selectionCriteria = new SelectionCriteria
            {
                PeriodStart = criteria.StartFiscalPeriod,
                PeriodStartYear = criteria.StartFiscalYear,
                PeriodEnd = criteria.EndFiscalPeriod,
                PeriodEndYear = criteria.EndFiscalYear,
                SelectionStartDate = criteria.StartDate,
                SelectionEndDate = criteria.EndDate   
            };

            var company = new Company
            {
                SystemID = context.Account.Id.ToString(),
                Name = context.Account.Name,
                RegistrationNumber = context.Account.RegistrationNumber,
                TaxRegistration = new TaxRegistration
                {
                    Number = context.Account.TaxRegistrationNumber
                },
                Contact = new Contact
                {
                    Email = context.Account.Email,
                    Telephone = context.Account.Telephone
                }
            };

            var accountAddress = context.Account.Address;
            
            company.Addresses.Add(new Address
            {
                City = accountAddress.City,
                Country = accountAddress.Country,
                Number = accountAddress.Number,
                Region = accountAddress.Region,
                Street = accountAddress.Street,
                PostalCode = accountAddress.PostalCode,
                Type = AddressType.Street
            });
            
            return new Header
            {
                CreationTime = DateTime.UtcNow,
                Country = context.Account.Address.Country,
                Region = context.Account.Address.Region,
                
                // TODO(kevin): what to put here?
                SoftwareCompanyName = string.Empty,
                SoftwareName = context.SoftwareName,
                SoftwareVersion = context.SoftwareVersion,
                
                DefaultCurrencyCode = context.Account.Currency,
                
                // Version of the SAF-T file
                Version = "2.0",
                
                SelectionCriteria = selectionCriteria,
                Company = company
            };
        }
    }
}