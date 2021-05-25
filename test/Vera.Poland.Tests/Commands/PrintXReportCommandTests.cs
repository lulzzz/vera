using System.Threading.Tasks;
using Vera.Poland.Commands;
using Vera.Poland.Protocol;
using Xunit;
using static Assertive.DSL;

namespace Vera.Poland.Tests.Commands
{
  public class PrintXReportCommandTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task Will_Send_Print_X_Report_Command()
    {
      ResetPrinterWriteRawDataResponse();
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });

      var response = await  Run<PrintXReportCommand>();

      Assert(() => response.Success);
      AssertCommandSentToPrinter();
    }

    [Fact]
    public async Task Will_Return_Error()
    {
      ResetPrinterWriteRawDataResponse();
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Nak });

      var response = await  Run<PrintXReportCommand>();

      Assert(() => !response.Success);
      AssertCommandSentToPrinter();
    }

    private void AssertCommandSentToPrinter()
    {
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand =
      new[]
        {
          FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.O, FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe
        };

      var expectedCommandString = EncodingHelper.Decode(expectedCommand);
      Assert(() => expectedCommandString == fullCommandString);
    }
  }
}