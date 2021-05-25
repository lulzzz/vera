using System.Threading.Tasks;
using Vera.Poland.Commands.ProtectedMemory;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.ProtectedMemory
{
  public class FinishPrintoutCommandTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task Will_Send_Correct_Command_To_Printer()
    {
      ResetPrinterWriteRawDataResponse();
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });

      var response = await  Run<FinishPrintoutCommand>();

      Assert.True(response.Success);

      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = new[]
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.H,
        FiscalPrinterDividers.E, FiscalPrinterDividers.E, FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfe
      };
      var expectedCommandString = EncodingHelper.Decode(expectedCommand);

      Assert.Equal(expectedCommandString, fullCommandString);
    }
  }
}