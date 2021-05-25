using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Poland.Commands.PredefinedReferencePrintouts;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.PredefinedReferencePrintouts
{
  public class EndPredefinedPrintoutCommandTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task Will_Send_Correct_Command_To_Printer()
    {
      ResetPrinterWriteRawDataResponse();
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });

      var response = await  Run<EndPredefinedPrintoutCommand>();

      Assert.True(response.Success);

      GetExpectedCommand();
    }

    private void GetExpectedCommand()
    {
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.N,
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfe
      };

      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());
      Assert.Equal(expectedCommandString, fullCommandString);
    }
  }
}