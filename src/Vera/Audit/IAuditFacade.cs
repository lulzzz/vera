using System;
using System.Threading.Tasks;
using Vera.Audit.Extract;
using Vera.Stores;

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
            // TODO(kevin): gather all the unique customers, employees, taxtable etc.

            // var audit = await CreateAudit(context, criteria);
            // var result = _transformer.Transform(context, criteria, audit);
            //
            // await _archive.Archive(criteria, result);
        }

        // private async Task<StandardAuditFileTaxation.Audit> CreateAudit(AuditContext context, AuditCriteria criteria)
        // {
        //     var audit = new StandardAuditFileTaxation.Audit(CreateHeader(context, criteria));
        //
        //     // TODO(kevin): move to factory?
        //     var extractors = new IAuditDataExtractor[]
        //     {
        //         new CustomerAuditDataExtractor(),
        //         new PaymentAuditDataExtractor(),
        //         new ProductAuditDataExtractor(),
        //         new TaxTableAuditExtractor(),
        //         new InvoiceAuditDataExtractor(),
        //         new PaymentAuditDataExtractor()
        //     };
        //
        //     // Extract data from all the invoices
        //     await foreach (var invoice in _invoiceStore.List(criteria))
        //     {
        //         foreach (var e in extractors)
        //         {
        //             e.Extract(invoice);
        //         }
        //     }
        //
        //     // Apply the extracted data to the auditing model
        //     foreach (var e in extractors)
        //     {
        //         e.Apply(audit);
        //     }
        //
        //     return audit;
        // }
    }
}