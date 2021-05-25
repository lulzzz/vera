namespace Vera.Poland.Models.Requests
{
  public class PrintVatRegistrationNumberRequest : PrinterRequest
  {
    public string CustomerNip { get; set; }
  }
}