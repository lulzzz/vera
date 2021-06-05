using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Vera.Extensions;
using Vera.Models;
using Vera.Stores;

namespace Vera.EventLogs
{
    public class EventLogCreator : IEventLogCreator
    {
        private readonly IEventLogStore _eventLogStore;
        private readonly IRegisterStore _registerStore;

        public EventLogCreator(IEventLogStore eventLogStore, IRegisterStore registerStore)
        {
            _eventLogStore = eventLogStore;
            _registerStore = registerStore;
        }

        public async Task Create(EventLog log)
        {
            var validationResult = Validate(log);
            if (validationResult != null)
            {
                throw new ValidationException(validationResult);
            }

            if (!string.IsNullOrEmpty(log.RegisterSystemId) && !log.RegisterId.HasValue)
            {
                var register = await _registerStore.GetBySystemIdAndSupplierId(log.Supplier.Id, log.RegisterSystemId);
                log.RegisterId = register.Id;
            }

            await _eventLogStore.Store(log);
        }

        private static string Validate(EventLog log)
        {
            if (log.Supplier == null)
            {
                return "missing supplier";
            }

            return log.Type switch
            {
                EventLogType.None => "missing event log type",
                EventLogType.CloseCashDrawer when string.IsNullOrEmpty(log.RegisterSystemId) =>
                    "require register systemId for cash drawer event logs",
                _ => null
            };
        }
    }
}
