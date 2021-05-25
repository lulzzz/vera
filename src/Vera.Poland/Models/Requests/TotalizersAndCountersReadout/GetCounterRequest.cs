using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Requests.TotalizersAndCountersReadout
{
  public class GetCounterRequest : PrinterRequest
  {
    public CountersReadoutEnum Counter { get; set; }
  }
}