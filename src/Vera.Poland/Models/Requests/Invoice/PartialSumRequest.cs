namespace Vera.Poland.Models.Requests.Invoice
{
  public class PartialSumRequest : PrinterRequest
  {
    public decimal PartialSum { get; set; }
  }
}