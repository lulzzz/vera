using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Requests.Invoice
{
  public class ReadoutDiscountAmountDistributionRequest: PrinterRequest
  {
    public SimpleSumDiscountAction Action { get; set; }

    public decimal Amount { get; set; }
  }
}