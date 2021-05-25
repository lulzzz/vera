using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Responses.TotalizersAndCountersReadout
{
  public class GetTotalizerResponse : PrinterResponse
  {
    public TotalizerOrCounterType TotalizerType { get; set; }
    public double TotalizerValue { get; set; }
  }
}