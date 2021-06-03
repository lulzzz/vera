using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vera.Audits;
using Vera.Azure.Extensions;
using Vera.Models;
using Vera.Stores;

namespace Vera.Azure.Stores
{
    public sealed class CosmosInvoiceStore : IInvoiceStore
    {
        private readonly Container _container;

        public CosmosInvoiceStore(Container container)
        {
            _container = container;
        }

        public async Task Store(Invoice invoice)
        {
            var byNumber = new Document<Invoice>(
                i => i.Id,
                i => PartitionKeyByNumber(i.AccountId, i.Number),
                invoice
            );

            await _container.CreateItemAsync(byNumber, new PartitionKey(byNumber.PartitionKey));
        }

        public Task<Invoice> GetByNumber(Guid accountId, string number)
        {
            var queryable = _container.GetItemLinqQueryable<Document<Invoice>>(requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(PartitionKeyByNumber(accountId, number)),
                MaxItemCount = 1
            });

            return queryable.FirstOrDefault();
        }

        public Task<ICollection<Invoice>> List(AuditCriteria criteria)
        {
            var queryable = _container.GetItemLinqQueryable<Document<Invoice>>()
                .Where(x => 
                    x.Value.AccountId == criteria.AccountId && 
                    x.Value.Supplier.Id == criteria.SupplierId && 
                    x.Value.Date >= criteria.StartDate && 
                    x.Value.Date <= criteria.EndDate
                );

            if (!string.IsNullOrEmpty(criteria.RegisterId))
            {
                queryable = queryable.Where(x => x.Value.RegisterId == criteria.RegisterId);
            }

            return queryable.ToListAsync();
        }

        public async Task Delete(Invoice invoice)
        {
            var response = await _container.DeleteItemStreamAsync(
                invoice.Id.ToString(),
                new PartitionKey(PartitionKeyByNumber(invoice.AccountId, invoice.Number))
            );

            response.EnsureSuccessStatusCode();
        }

        private static string PartitionKeyByNumber(Guid accountId, string invoiceNumber) => $"{accountId}#N#{invoiceNumber}";
    }
}