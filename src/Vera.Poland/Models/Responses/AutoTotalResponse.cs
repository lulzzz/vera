namespace Vera.Poland.Models.Responses
{
  public class AutoTotalResponse: PrinterResponse
  {
    public decimal Total { get; set; }

    public bool ParsedOk { get; set; }

    public string Raw { get; set; }
  }
}