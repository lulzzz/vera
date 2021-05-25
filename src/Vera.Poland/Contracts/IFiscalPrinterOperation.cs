using Vera.Poland.Models.Requests;

namespace Vera.Poland.Contracts
{
  public interface IFiscalPrinterOperation<in T>:
    IFiscalPrinterValidator<T>,
    IFiscalPrinterRequestBuilder<T>
    where T : PrinterRequest, new()
  {
  }
}