using System;
using Google.Protobuf.WellKnownTypes;
using Vera.Grpc;

namespace Vera.Host.Mapping
{
    public static class PeriodExtensions
    {
        public static Period Pack(this Vera.Models.Period period)
        {
            var closingUtc = period.Closing == DateTime.MinValue ? DateTime.SpecifyKind(period.Closing, DateTimeKind.Utc) : period.Closing;
            
            var result = new Period
            {
                Id = period.Id.ToString(),
                Opening = period.Opening.ToTimestamp(),
                Closing = closingUtc.ToTimestamp(),
                SupplierSystemId = period.Supplier?.SystemId
            };

            return result;
        }
    }
}
