using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Vera.Audits;
using Vera.Bootstrap;
using Vera.Grpc;
using Vera.Stores;
using Vera.WebApi.Security;

namespace Vera.WebApi.Services
{
    [Authorize]
    public class ArchiveService : Grpc.ArchiveService.ArchiveServiceBase
    {
        private readonly IAccountStore _accountStore;
        private readonly IInvoiceStore _invoiceStore;
        private readonly IAccountComponentFactoryCollection _accountComponentFactoryCollection;

        public ArchiveService(
            IAccountStore accountStore,
            IInvoiceStore invoiceStore,
            IAccountComponentFactoryCollection accountComponentFactoryCollection
        )
        {
            _accountStore = accountStore;
            _invoiceStore = invoiceStore;
            _accountComponentFactoryCollection = accountComponentFactoryCollection;
        }

        public override async Task<ArchiveReply> Archive(ArchiveRequest request, ServerCallContext context)
        {
            var account = await context.ResolveAccount(_accountStore, request.AccountId);
            var factory = _accountComponentFactoryCollection.GetComponentFactory(account);

            // TODO(kevin): simply create an audit entry and return that
            // TODO(kevin): new service to retrieve status of the audit

            // TODO(kevin): fix dependencies
            var processor = new AuditProcessor(_invoiceStore, null, null, factory);

            // TODO(kevin): want this to happen async, so it can run in the background
            // TODO(kevin): do something with result to return a reply
            // await processor.Process(new AuditCriteria
            // {
            //     AccountId = account.Id,
            //     StartDate = request.Start.ToDateTime(),
            //     EndDate = request.End.ToDateTime()
            // });

            return new ArchiveReply();
        }
    }
}