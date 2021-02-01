using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Stores;

namespace Vera.Audits
{
    public interface IAuditProcessor
    {
        Task Process(Account account, Audit audit);
    }

    public class AuditProcessor : IAuditProcessor
    {
        private readonly IInvoiceStore _invoiceStore;
        private readonly IBlobStore _blobStore;
        private readonly IAuditStore _auditStore;
        private readonly IComponentFactory _componentFactory;

        public AuditProcessor(
            IInvoiceStore invoiceStore,
            IBlobStore blobStore,
            IAuditStore auditStore,
            IComponentFactory componentFactory
        )
        {
            _invoiceStore = invoiceStore;
            _blobStore = blobStore;
            _auditStore = auditStore;
            _componentFactory = componentFactory;
        }

        public async Task Process(Account account, Audit audit)
        {
            // TODO(kevin): audit should be created earlier - updated in this section with relevant information
            // TODO(kevin): ^ can probably fetch the context for the generation from this as well

            // TODO(kevin): split up the generation per month, per supplier
            // TODO(kevin): ^ make output a zip if more then 1 file

            // TODO(kevin): how to get all the suppliers for an account? do we even need to?
            var criteria = new AuditCriteria
            {
                AccountId = account.Id,
                SupplierSystemId = audit.SupplierSystemId,
                StartDate = audit.Start,
                EndDate = audit.End
            };

            // TODO(kevin): list/page per supplier per month
            var invoices = await _invoiceStore.List(criteria);

            var context = new AuditContext
            {
                Invoices = invoices,
                Account = account
            };

            await using var stream = File.Create(Path.GetTempFileName(), 4096, FileOptions.DeleteOnClose);

            var writer = _componentFactory.CreateAuditWriter();

            // TODO(kevin): pass extra context so the writer can split it up in multiple files?
            await writer.Write(context, criteria, stream);

            // TODO: store should also come from factory - because it could be in an external system (fiskaly)
            var location = await _blobStore.Store(account.Id, stream);

            audit.Location = location;

            await _auditStore.Update(audit);
        }
    }
}