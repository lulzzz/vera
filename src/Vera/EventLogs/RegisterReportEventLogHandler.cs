using System;
using System.Threading.Tasks;
using Vera.Dependencies;
using Vera.Dependencies.Handlers;
using Vera.Models;
using Vera.Stores;

namespace Vera.EventLogs
{
    public class RegisterReportEventLogHandler : HandlerChain<RegisterReport>
    {
        private readonly IEventLogStore _eventLogStore;
        private readonly ISupplierStore _supplierStore;
        private readonly IDateProvider _dateProvider;

        public RegisterReportEventLogHandler(
            IEventLogStore eventLogStore,
            ISupplierStore supplierStore,
            IDateProvider dateProvider)
        {
            _eventLogStore = eventLogStore;
            _supplierStore = supplierStore;
            _dateProvider = dateProvider;
        }

        public override Task Handle(RegisterReport entity)
        {
            return WillHandleReport(entity.Type) ? HandleEventLogEvent(entity) : base.Handle(entity);
        }

        private async Task HandleEventLogEvent(RegisterReport entity)
        {
            var supplier = await _supplierStore.Get(entity.Account.Id, entity.SupplierId);
            if (supplier == null)
            {
                throw new NullReferenceException($"Supplier {entity.SupplierId} does not exist");
            }

            var eventLog = new EventLog
            {
                Date = _dateProvider.Now,
                Supplier = supplier,
                RegisterId = entity.RegisterId,
                Type = entity.Type switch
                {
                    RegisterReportType.Current => EventLogType.CurrentRegisterReportCreated,
                    RegisterReportType.EndOfDay => EventLogType.EndOfDayRegisterReportCreated,
                    _ => throw new ArgumentOutOfRangeException(nameof(entity), $"{nameof(entity)}.{nameof(entity.Type)}")
                },
                ReportNumber = entity.Number,
                EmployeeId = entity.EmployeeId
            };

            await _eventLogStore.Store(eventLog);
        }

        private static bool WillHandleReport(RegisterReportType type) =>
            type is RegisterReportType.Current or RegisterReportType.EndOfDay;
    }
}
