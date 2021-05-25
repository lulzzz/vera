using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Requests.TotalizersAndCountersReadout
{
  public class GetTotalizerRequest : PrinterRequest
  {
    public TotalizersReadoutEnum Totalizer { get; set; }
  }
}