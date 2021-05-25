
namespace Vera.Poland.Models.Requests.Invoice
{
  public class AddTransactionLineRequest: OrderLinePrinterRequest
  {
    public string Unit { get; set; }

    public string? Comment1 { get; set; }

    public string? Comment2 { get; set; }
  }
}
