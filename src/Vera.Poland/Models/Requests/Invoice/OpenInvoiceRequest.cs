namespace Vera.Poland.Models.Requests.Invoice
{
  public class OpenInvoiceRequest: PrinterRequest
  {
    public string? InvoiceIdentifier { get; set; }
  }
}
