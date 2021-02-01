using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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

            // TODO(kevin): split up the generation per month and per supplier

            var criteria = new AuditCriteria
            {
                AccountId = account.Id,
                SupplierSystemId = audit.SupplierSystemId,
                StartDate = audit.Start,
                EndDate = audit.End
            };

            var result = _invoiceStore.List(criteria);
            var invoices = new List<Invoice>();

            await foreach (var invoice in result)
            {
                invoices.Add(invoice);
            }

            var context = new AuditContext
            {
                Invoices = invoices,
                Account = account
                // TODO(kevin): map other properties
            };

            await using var stream = File.Create(Path.GetTempFileName(), 4096, FileOptions.DeleteOnClose);

            var writer = _componentFactory.CreateAuditWriter();
            await writer.Write(context, criteria, stream);

            // TODO: store should also come from factory - because it could be in an external system (fiskaly)
            var location = await _blobStore.Store(account.Id, stream);

            audit.Location = location;

            await _auditStore.Update(audit);
        }
    }
}