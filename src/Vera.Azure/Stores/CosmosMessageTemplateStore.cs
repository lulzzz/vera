using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Vera.Models;
using Vera.Stores;

namespace Vera.Azure.Stores
{
    public sealed class CosmosMessageTemplateStore: IMessageTemplateStore
    {
        private readonly Container _container;
        private const string DocumentType = "messageTemplate";


        public CosmosMessageTemplateStore(Container container)
        {
            _container = container;
        }
        
        public async Task Store(MessageTemplate messageTemplate)
        {
            var toCreate = ToDocument(messageTemplate);
            
            await _container.CreateItemAsync(
                toCreate,
                new PartitionKey(toCreate.PartitionKey)
            );
        }
        
        public Task Update(MessageTemplate messageTemplate)
        {
            var document = ToDocument(messageTemplate);

            return _container.ReplaceItemAsync(
                document,
                document.Id.ToString(),
                new PartitionKey(document.PartitionKey)
            );
        }

        public async Task<MessageTemplate> Get(string accountId, Guid id)
        {
            try
            {
                var document = await _container.ReadItemAsync<TypedDocument<MessageTemplate>>(
                    id.ToString(),
                    new PartitionKey(accountId)
                );

                return document.Resource.Value;
            }
            catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private static TypedDocument<MessageTemplate> ToDocument(MessageTemplate messageTemplate)
        {
            return new(
                c => c.Id,
                c => c.AccountId.ToString(),
                messageTemplate,
                DocumentType
            );
        }
    }
}