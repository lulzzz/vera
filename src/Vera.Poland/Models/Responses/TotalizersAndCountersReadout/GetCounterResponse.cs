using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Responses.TotalizersAndCountersReadout
{
  public class GetCounterResponse : PrinterResponse
  {
    public TotalizerOrCounterType CounterType { get; set; }
    public double CounterValue { get; set; }
  }
}