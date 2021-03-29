using Google.Protobuf.WellKnownTypes;
using System;

namespace Vera.Grpc.Models
{
    public static class PeriodExtensions
    {
        public static Period Pack(this Vera.Models.Period period)
        {
            var closingUtc = period.Closing == DateTime.MinValue ? DateTime.SpecifyKind(period.Closing, DateTimeKind.Utc) : period.Closing;
            var result = new Period
            {
                Opening = period.Opening.ToTimestamp(),
                Closing = closingUtc.ToTimestamp(),
                SupplierSystemId = period.Supplier?.SystemId
            };

            return result;
        }
    }
}
