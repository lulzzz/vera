using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vera.Azure.Stores;
using Vera.Portugal.Models;

namespace Vera.Portugal.Stores
{
    public class CosmosWorkingDocumentStore : IWorkingDocumentStore
    {
        private const string DocumentType = "working-document";

        private readonly Container _container;

        public CosmosWorkingDocumentStore(Container container)
        {
            _container = container;
        }

        public Task Store(WorkingDocument wd)
        {
            var typedDocument = ToDocument(wd);

            return _container.CreateItemAsync(typedDocument, new PartitionKey(typedDocument.PartitionKey));
        }

        public Task Delete(WorkingDocument wd)
        {
            var typedDocument = ToDocument(wd);

            return _container.DeleteItemAsync<TypedDocument<WorkingDocument>>(
                typedDocument.Id.ToString(), 
                new PartitionKey(typedDocument.PartitionKey));
        }

        public async Task<ICollection<WorkingDocument>> List(Guid invoiceId)
        {
            var query = new StringBuilder(@"
select value wd
 from c[""Value""] wd
where wd.InvoiceId = @invoiceId");

            var definition = new QueryDefinition(query.ToString());
            definition.WithParameter("@invoiceId", invoiceId);

            var iterator = _container.GetItemQueryIterator<WorkingDocument>(definition);

            var workingDocuments = new List<WorkingDocument>();

            while (iterator.HasMoreResults)
            {
                var results = await iterator.ReadNextAsync();
                workingDocuments.AddRange(results);
            }

            return workingDocuments;
        }

        private static TypedDocument<WorkingDocument> ToDocument(WorkingDocument wd)
        {
            return new(
                d => d.Id,
                d => d.SupplierSystemId,
                wd,
                DocumentType
            );
        }
    }
}
