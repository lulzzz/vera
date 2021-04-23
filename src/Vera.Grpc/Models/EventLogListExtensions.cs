using System;
using Vera.EventLogs;

namespace Vera.Grpc.Models
{
    public static class EventLogListExtensions
    {
        public static EventLogCriteria BuildListCriteria(this ListEventLogRequest request,
            Guid accountId)
        {
            var unpackedType = request.Type.Unpack();

            Vera.Models.EventLogType? type = unpackedType == Vera.Models.EventLogType.None ? null : unpackedType;

            var criteria = new EventLogCriteria
            {
                EndDate = request.EndDate == null ? DateTime.MaxValue : request.EndDate.ToDateTime(),

                StartDate = request.StartDate == null ? DateTime.MinValue : request.StartDate.ToDateTime(),

                Type = type,

                AccountId = accountId
            };

            return criteria;
        }
    }
}