using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Requests.Invoice
{
  public class MutateDiscountTransactionLineRequest: PrinterRequest
  {
    public string ProductName { get; set; }

    public DiscountType Type { get; set; }

    public DiscountActionType ActionType { get; set; }

    public DiscountAction Action { get; set; }

    /// <summary>
    /// Total discount value - we only use this atm
    /// </summary>
    public decimal Value { get; set; }

    public VatClass Vat { get; set; }

    /// <summary>
    /// Value in percentage - can't use this on an invoice
    /// </summary>
    public decimal? ValueInPercentage { get; set; }
  }
}