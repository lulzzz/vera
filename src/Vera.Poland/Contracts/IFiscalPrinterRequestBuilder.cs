using System.Collections.Generic;
using Vera.Poland.Models.Requests;

namespace Vera.Poland.Contracts
{
  public interface IFiscalPrinterRequestBuilder<in T>
    where T : PrinterRequest, new()
  {
    void BuildRequest(T input, List<byte> request);
  }
}