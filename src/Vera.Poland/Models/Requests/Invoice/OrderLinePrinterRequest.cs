using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Requests.Invoice
{
  public class OrderLinePrinterRequest : PrinterRequest
  {
    /// <summary>
    /// Order line product name which will be displayed
    /// on the receipt/invoice
    /// </summary>
    public string ProductName { get; set; }

    /// <summary>
    /// Order line quantity
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Order line unit price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Order line total value (unit price * quantity)
    /// </summary>
    public decimal Value { get; set; }

    public VatClass Vat { get; set; }
  }
}