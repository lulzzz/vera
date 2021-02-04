using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Vera.Audits;
using Vera.Bootstrap;
using Vera.Grpc;
using Vera.Host.Security;
using Vera.Stores;

namespace Vera.Host.Services
{
    [Authorize]
    public class AuditService : Grpc.AuditService.AuditServiceBase
    {
        private readonly IAccountStore _accountStore;
        private readonly IInvoiceStore _invoiceStore;
        private readonly IBlobStore _blobStore;
        private readonly IAuditStore _auditStore;
        private readonly IAccountComponentFactoryCollection _accountComponentFactoryCollection;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;

        public AuditService(
            IAccountStore accountStore,
            IInvoiceStore invoiceStore,
            IBlobStore blobStore,
            IAuditStore auditStore,
            IAccountComponentFactoryCollection accountComponentFactoryCollection,
            IBackgroundTaskQueue backgroundTaskQueue
        )
        {
            _accountStore = accountStore;
            _invoiceStore = invoiceStore;
            _blobStore = blobStore;
            _auditStore = auditStore;
            _accountComponentFactoryCollection = accountComponentFactoryCollection;
            _backgroundTaskQueue = backgroundTaskQueue;
        }

        public override async Task<CreateAuditReply> Create(CreateAuditRequest request, ServerCallContext context)
        {
            var account = await context.ResolveAccount(_accountStore, request.AccountId);
            var factory = _accountComponentFactoryCollection.GetComponentFactory(account);

            // TODO(kevin): validate the request, start < end and supplier is not empty/nil

            var audit = await _auditStore.Create(new AuditCriteria
            {
                AccountId = account.Id,
                SupplierSystemId = request.SupplierSystemId,
                StartDate = request.StartDate.ToDateTime(),
                EndDate = request.EndDate.ToDateTime()
            });

            var archiver = new AuditArchiver(
                _invoiceStore,
                _blobStore,
                _auditStore,
                factory
            );

            _backgroundTaskQueue.Queue(_ => archiver.Archive(account, audit));

            return new CreateAuditReply
            {
                AuditId = audit.Id.ToString()
            };
        }

        public override async Task<GetAuditReply> Get(GetAuditRequest request, ServerCallContext context)
        {
            var account = await context.ResolveAccount(_accountStore, request.AccountId);
            var audit = await _auditStore.Get(account.Id, Guid.Parse(request.AuditId));

            return new GetAuditReply
            {
                StartDate = audit.Start.ToTimestamp(),
                EndDate = audit.End.ToTimestamp(),
                Location = audit.Location ?? string.Empty
            };
        }
    }
}