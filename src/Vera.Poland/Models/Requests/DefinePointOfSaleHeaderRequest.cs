using System.Collections.Generic;

namespace Vera.Poland.Models.Requests
{
  public class DefinePointOfSaleHeaderRequest : PrinterRequest
  {
    public List<string> Lines { get; set; }
  }
}