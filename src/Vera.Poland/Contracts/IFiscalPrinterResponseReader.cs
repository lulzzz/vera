using Vera.Poland.Models.Responses;

namespace Vera.Poland.Contracts
{
  public interface IFiscalPrinterResponseReader<out TK>
    where TK : PrinterResponse, new()
  {
    TK ReadResponse(byte[] printerRawResponse);
  }
}