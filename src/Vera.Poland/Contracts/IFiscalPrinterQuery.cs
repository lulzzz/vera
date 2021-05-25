using System.Collections.Generic;
using Vera.Poland.Models.Requests;
using Vera.Poland.Models.Responses;

namespace Vera.Poland.Contracts
{
  public interface IFiscalPrinterQuery<in T, out TK>
    : IFiscalPrinterOperation<T>, IFiscalPrinterResponseReader<TK>
    where T : PrinterRequest, new()
    where TK: PrinterResponse, new()
  {
  }

  public interface IFiscalPrinterQuery<out TK>
  : IFiscalPrinterResponseReader<TK>
    where TK : PrinterResponse, new()
  {
    void BuildRequest(List<byte> request);
  }
}