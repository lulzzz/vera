using System.Collections.Generic;
using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Requests.Invoice
{
  public class MutateReceiptPaymentAndFooterRequest : PrinterRequest
  {
    public SupportedPaymentAndFooterTypes Index { get; set; }

    public List<ParameterModel> Parameters { get; set; }
  }

  public class ParameterModel
  {
    public string Name { get; set; }
    public ParameterTypeEnum Type { get; set; }
  }
}