using System.Threading.Tasks;
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

            var audit = await _auditStore.Create(new AuditCriteria
            {
                AccountId = account.Id,
                // TODO(kevin): optional parameter?
                // SupplierSystemId = request...
                StartDate = request.StartDate.ToDateTime(),
                EndDate = request.EndDate.ToDateTime()
            });

            var processor = new AuditProcessor(
                _invoiceStore,
                _blobStore,
                _auditStore,
                factory
            );

            _backgroundTaskQueue.Queue(_ => processor.Process(account, audit));

            return new CreateAuditReply
            {
                AuditId = audit.Id.ToString()
            };
        }

        // TODO(kevin): service to fetch (status) of audit - also used to fetch location so client can download
    }
}