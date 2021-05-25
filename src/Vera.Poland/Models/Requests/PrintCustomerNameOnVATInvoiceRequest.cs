using System.Collections.Generic;

namespace Vera.Poland.Models.Requests
{
  public class PrintCustomerNameOnVatInvoiceRequest : PrinterRequest
  {
    public List<string> CustomerNameLines { get; set; }
  }
}