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

        public override async Task Handle(RegisterReport entity)
        {
            if (!CanContinue(entity.ReportType))
            {
                await base.Handle(entity);
            } 
            else
            {
                await HandleEventLogEvent(entity);
            }
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
                Type = entity.ReportType switch
                {
                    ReportType.X => EventLogType.XReport,
                    ReportType.Z => EventLogType.ZReport,
                    _ => throw new ArgumentOutOfRangeException()
                }
            };

            await _eventLogStore.Store(eventLog);
        }

        private static bool CanContinue(ReportType reportType) => reportType == ReportType.X || reportType == ReportType.Z;
    }
}
