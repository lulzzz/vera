namespace Vera.Poland.Models.Requests.Invoice
{
  public class CancelReceiptOrInvoiceRequest : PrinterRequest
  {
    public decimal Total { get; set; } // determines the current sum of the receipt.

  }
}