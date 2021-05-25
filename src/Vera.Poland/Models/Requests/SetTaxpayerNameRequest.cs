using System.Collections.Generic;

namespace Vera.Poland.Models.Requests
{
  public class SetTaxpayerNameRequest : PrinterRequest
  {
    /// <summary>
    /// Tax payer lines to be set. Must be a maximum of 6 lines
    /// </summary>
    public List<string> TaxpayerLines { get; set; }
  }
}