using System;
using EventLogType = Vera.Grpc.EventLogType;

namespace Vera.Host.Mapping
{
    public static class EventLogTypeExtensions
    {
        public static Vera.Models.EventLogType Unpack(this EventLogType eventLogType)
        {
            return eventLogType switch
            {
                EventLogType.None => Vera.Models.EventLogType.None,
                EventLogType.AppStart => Vera.Models.EventLogType.AppStart,
                EventLogType.Login => Vera.Models.EventLogType.Login,
                EventLogType.Logout => Vera.Models.EventLogType.Logout,
                EventLogType.OpenCashDrawer => Vera.Models.EventLogType.OpenCashDrawer,
                EventLogType.CloseCashDrawer => Vera.Models.EventLogType.CloseCashDrawer,
                EventLogType.XReport => Vera.Models.EventLogType.XReport,
                EventLogType.ZReport => Vera.Models.EventLogType.ZReport,
                EventLogType.ReceiptPrinted => Vera.Models.EventLogType.ReceiptPrinted,
                EventLogType.ReceiptReprinted => Vera.Models.EventLogType.ReceiptReprinted,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static EventLogType Pack(this Vera.Models.EventLogType eventLogType)
        {
            return eventLogType switch
            {
                Vera.Models.EventLogType.None => EventLogType.None,
                Vera.Models.EventLogType.AppStart => EventLogType.AppStart,
                Vera.Models.EventLogType.Login => EventLogType.Login,
                Vera.Models.EventLogType.Logout => EventLogType.Logout,
                Vera.Models.EventLogType.OpenCashDrawer => EventLogType.OpenCashDrawer,
                Vera.Models.EventLogType.CloseCashDrawer => EventLogType.CloseCashDrawer,
                Vera.Models.EventLogType.XReport => EventLogType.XReport,
                Vera.Models.EventLogType.ZReport => EventLogType.ZReport,
                Vera.Models.EventLogType.ReceiptPrinted => EventLogType.ReceiptPrinted,
                Vera.Models.EventLogType.ReceiptReprinted => EventLogType.ReceiptReprinted,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}