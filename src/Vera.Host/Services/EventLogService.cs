using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Vera.Grpc;
using Vera.Host.Mapping;
using Vera.Host.Security;
using Vera.Stores;

namespace Vera.Host.Services
{
    [Authorize]
    public class EventLogService : Grpc.EventLogService.EventLogServiceBase
    {
        private readonly IEventLogStore _eventLogStore;
        private readonly ISupplierStore _supplierStore;

        public EventLogService(IEventLogStore eventLogStore, ISupplierStore supplierStore)
        {
            _eventLogStore = eventLogStore;
            _supplierStore = supplierStore;
        }

        public override async Task<CreateEventLogReply> Create(CreateEventLogRequest request, ServerCallContext context)
        {
            var eventLog = request.Eventlog.Unpack();

            ValidateType(eventLog);

            var supplier = await context.ResolveSupplier(_supplierStore, request.Eventlog.SupplierSystemId);

            eventLog.Supplier = supplier;

            await _eventLogStore.Store(eventLog);

            return new CreateEventLogReply
            {
                Id = eventLog.Id.ToString()
            };
        }

        public override async Task<ListEventLogReply> List(ListEventLogRequest request, ServerCallContext context)
        {
            var accountId = context.GetAccountId();
            var supplier = await context.ResolveSupplier(_supplierStore, request.SupplierSystemId);
            
            var criteria = request.BuildListCriteria(accountId, supplier.Id);

            var eventLogs = await _eventLogStore.List(criteria);

            var reply = new ListEventLogReply();

            reply.EventLogs.AddRange(eventLogs.Pack());

            return reply;
        }

        private static void ValidateType(Models.EventLog eventLog)
        {
            switch (eventLog.Type)
            {
                case Vera.Models.EventLogType.None:
                    FailedPrecondition("Type required for event");
                    break;
                case Vera.Models.EventLogType.CloseCashDrawer when string.IsNullOrEmpty(eventLog.RegisterId):
                    FailedPrecondition($"Register required for event {eventLog.Type}");
                    break;
            }
        }

        private static void FailedPrecondition(string detail)
        {
            throw new RpcException(
                new Status(StatusCode.FailedPrecondition, detail));
        }
    }
}
