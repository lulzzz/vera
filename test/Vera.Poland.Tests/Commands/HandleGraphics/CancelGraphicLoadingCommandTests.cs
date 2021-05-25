using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Poland.Commands.HandleGraphics;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.HandleGraphics
{
  public class CancelGraphicLoadingCommandTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task Will_Send_Correct_Command_To_Device()
    {
      ResetPrinterWriteRawDataResponse();
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });

      var result = await Run<CancelGraphicLoadingCommand>();

      Assert.True(result.Success);

      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.OpenParenthesis, FiscalPrinterDividers.L,
        FiscalPrinterDividers.C, FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe
      };

      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());
      Assert.Equal(expectedCommandString, fullCommandString);
    }
  }
}