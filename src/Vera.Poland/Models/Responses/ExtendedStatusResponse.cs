using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Responses
{
  public class ExtendedStatusResponse : PrinterResponse
  {
    public FiscalStatus FiscalStatus { get; set; }
    public PrinterMechanismStatus PrinterMechanismStatus { get; set; }
  }
}