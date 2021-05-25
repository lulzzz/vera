using System.Threading.Tasks;
using Vera.Poland.Commands;
using Vera.Poland.Models;
using Xunit;
using static Assertive.DSL;

namespace Vera.Poland.Tests.Commands
{
  public class ListPrinterErrorsQueryTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task LastPrinterErrorTest()
    {
      ResetPrinterWriteRawDataResponse();

      var lastPrinterError = ProducePrinterLastErrorMockResponse(0x2000);
      MockSinglePrinterResponse(lastPrinterError);

      var result = await  Run<ListPrinterErrorQuery, PrinterError>();

      Assert(() => !result.ResponseMalformed);
      Assert(() => result.Success);
      Assert(() => result.Code == 0x2000);
    }
  }
}