using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Vera.EventLogs;
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
        private readonly IRegisterStore _registerStore;
        private readonly IEventLogCreator _eventLogCreator;

        public EventLogService(
            IEventLogStore eventLogStore,
            ISupplierStore supplierStore,
            IRegisterStore registerStore,
            IEventLogCreator eventLogCreator
        )
        {
            _eventLogStore = eventLogStore;
            _supplierStore = supplierStore;
            _registerStore = registerStore;
            _eventLogCreator = eventLogCreator;
        }

        public override async Task<CreateEventLogReply> Create(CreateEventLogRequest request, ServerCallContext context)
        {
            var log = request.Eventlog;

            var supplier = await context.ResolveSupplier(_supplierStore, log.SupplierSystemId);

            var newLog = new Vera.Models.EventLog
            {
                Date = log.Timestamp.ToDateTime(),
                Supplier = supplier,
                RegisterSystemId = log.RegisterSystemId,
                EmployeeId = log.EmployeeId,
                Type = log.Type.Unpack()
            };

            await _eventLogCreator.Create(newLog);

            return new CreateEventLogReply
            {
                Id = newLog.Id.ToString()
            };
        }

        public override async Task<ListEventLogReply> List(ListEventLogRequest request, ServerCallContext context)
        {
            var accountId = context.GetAccountId();
            var supplier = await context.ResolveSupplier(_supplierStore, request.SupplierSystemId);

            var unpackedType = request.Type.Unpack();

            Vera.Models.EventLogType? type = unpackedType == Models.EventLogType.None ? null : unpackedType;

            var criteria = new EventLogCriteria
            {
                EndDate = request.EndDate?.ToDateTime(),
                StartDate = request.StartDate?.ToDateTime(),
                Type = type,
                AccountId = accountId,
                SupplierId = supplier.Id
            };

            if (!string.IsNullOrEmpty(request.RegisterSystemId))
            {
                var register = await _registerStore.GetBySystemIdAndSupplierId(supplier.Id, request.RegisterSystemId);
                criteria.RegisterId = register.Id;
            }

            var eventLogs = await _eventLogStore.List(criteria);

            var reply = new ListEventLogReply();

            reply.EventLogs.AddRange(eventLogs.Pack());

            return reply;
        }
    }
}
