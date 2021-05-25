using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Requests.Invoice
{
  public class MutatePercentageDiscountForOneTaxRateRequest: PrinterRequest
  {
    public SimpleSumDiscountAction Action { get; set; }

    public DiscountType Type { get; set; }

    public VatClass Vat { get; set; }

    public decimal Value { get; set; }
  }
}