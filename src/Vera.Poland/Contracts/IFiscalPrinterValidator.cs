using Vera.Poland.Models.Requests;

namespace Vera.Poland.Contracts
{
  public interface IFiscalPrinterValidator<in T>
    where T : PrinterRequest, new()
  {
    void Validate(T input);
  }
}