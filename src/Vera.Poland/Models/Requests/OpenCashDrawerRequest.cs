namespace Vera.Poland.Models.Requests
{
  public class OpenCashDrawerRequest : PrinterRequest
  {
    public FiscalPrinterCashDrawer DrawerToOpen { get; set; }
  }
}