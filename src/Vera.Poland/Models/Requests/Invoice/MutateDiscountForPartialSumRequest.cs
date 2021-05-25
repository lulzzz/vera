using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Requests.Invoice
{
  public class MutateDiscountForPartialSumRequest : PrinterRequest
  {
    public SimpleSumDiscountAction Action { get; set; }

    public DiscountActionType ActionType { get; set; }

    public VatAmounts? Amounts { get; set; }

    public decimal Value { get; set; }
  }
}