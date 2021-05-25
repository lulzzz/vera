using System.Collections.Generic;

namespace Vera.Poland.Models.Requests.Invoice
{
  public class DefineTextOnVatInvoiceRequest : PrinterRequest
  {
    public string Description { get; set; }
    public List<string> TextLines { get; set; } // up to 5 lines of text, with length max 13 characters in the one line.
  }
}