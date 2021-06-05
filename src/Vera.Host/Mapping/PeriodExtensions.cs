using System;
using Google.Protobuf.WellKnownTypes;
using Vera.Grpc;

namespace Vera.Host.Mapping
{
    public static class PeriodExtensions
    {
        public static Period Pack(this Models.Period period)
        {
            var result = new Period
            {
                Id = period.Id.ToString(),
                Opening = period.Opening.ToTimestamp(),
                Closing = period.Closing?.ToTimestamp()
            };

            return result;
        }
    }
}
