using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Requests.Invoice
{
  public class MutatePercentageDiscountForPartialSumRequest : PrinterRequest
  {
    public SimpleSumDiscountAction Action { get; set; }

    public decimal Percentage { get; set; }
  }
}