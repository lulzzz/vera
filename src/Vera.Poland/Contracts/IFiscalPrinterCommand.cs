using System.Collections.Generic;
using Vera.Poland.Models.Requests;

namespace Vera.Poland.Contracts
{
  public interface IFiscalPrinterCommand<in T> : IFiscalPrinterOperation<T>
    where T : PrinterRequest, new()
  {

  }

  public interface IFiscalPrinterCommand
  {
    void BuildRequest(List<byte> request);
  }
}