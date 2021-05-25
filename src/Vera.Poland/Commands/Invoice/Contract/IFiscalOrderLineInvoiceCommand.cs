using Vera.Poland.Contracts;
using Vera.Poland.Models.Requests.Invoice;

namespace Vera.Poland.Commands.Invoice.Contract
{
  public interface IFiscalOrderLineInvoiceCommand<in TRequest>
    : IFiscalPrinterCommand<TRequest>
    where TRequest : OrderLinePrinterRequest, new()
  {
  }
}