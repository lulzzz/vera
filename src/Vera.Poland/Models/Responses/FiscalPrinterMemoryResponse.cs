namespace Vera.Poland.Models.Responses
{
  public class FiscalPrinterMemoryResponse : PrinterResponse
  {
    public bool FiscalMemoryAlmostFull { get; set; }
    public bool FiscalMemoryFull { get; set; }
  }
}