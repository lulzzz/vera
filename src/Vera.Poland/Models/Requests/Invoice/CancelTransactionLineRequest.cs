namespace Vera.Poland.Models.Requests.Invoice
{
  public class CancelTransactionLineRequest : OrderLinePrinterRequest
  {
    public string? Comment1 { get; set; }

    public string? Comment2 { get; set; }
  }
}