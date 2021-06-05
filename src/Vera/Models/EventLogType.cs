namespace Vera.Models
{
    public enum EventLogType
    {
        None,
        AppStart,
        Login,
        Logout,
        OpenCashDrawer,
        CloseCashDrawer,
        CurrentRegisterReportCreated,
        EndOfDayRegisterReportCreated,
        ReceiptPrinted,
        ReceiptReprinted
    }
}
