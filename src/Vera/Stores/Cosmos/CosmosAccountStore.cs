using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Vera.Models;

namespace Vera.Stores.Cosmos
{
    public class CosmosAccountStore : IAccountStore
    {
        private const string DocumentType = "account";

        private readonly Container _container;

        public CosmosAccountStore(Container container)
        {
            _container = container;
        }

        public Task Store(Account account)
        {
            var document = new AccountDocument(account);

            return _container.CreateItemAsync(document, new PartitionKey(document.PartitionKey));
        }

        public Task Update(Account account)
        {
            var document = new AccountDocument(account);

            return _container.ReplaceItemAsync(
                document,
                document.Id.ToString(),
                new PartitionKey(document.PartitionKey)
            );
        }

        public async Task<Account> Get(Guid companyId, Guid accountId)
        {
            var document = await _container.ReadItemAsync<AccountDocument>(
                accountId.ToString(),
                new PartitionKey(companyId.ToString())
            );


            return document.Resource?.Account;
        }

        public async Task<ICollection<Account>> GetByCompany(Guid companyId)
        {
            var definition = new QueryDefinition("select * from a where a.Type = @documentType")
                .WithParameter("@documentType", DocumentType);

            using var iterator = _container.GetItemQueryIterator<AccountDocument>(definition, requestOptions: new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(companyId.ToString())
            });

            var accounts = new List<Account>();

            while (iterator.HasMoreResults)
            {
                accounts.AddRange(
                    from result in await iterator.ReadNextAsync()
                    select result.Account
                );
            }

            return accounts;
        }

        private class AccountDocument
        {
            public AccountDocument() { }

            public AccountDocument(Account account)
            {
                Id = account.Id;
                Account = account;
                PartitionKey = account.CompanyId.ToString();
                Type = DocumentType;
            }

            [JsonProperty("id")]
            public Guid Id { get; set; }
            public Account Account { get; set; }
            public string PartitionKey { get; set; }
            public string Type { get; set; }
        }
    }
}