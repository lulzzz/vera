using System.Collections.Generic;
using System.Threading.Tasks;
using Assertive;
using Vera.Poland.Commands.Settings;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.Settings
{
  public class AbortSaveSettingTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task AbortSaveSettingCommand_Works()
    {
      SetupAckRespondingPrinter();

      var response = await  Run<AbortSaveSettingCommand>();
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand();
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      DSL.Assert(() => fullCommandString == expectedCommandString);
      DSL.Assert(() => response.Success);
    }

    private static List<byte> GetExpectedSentCommand()
    {
      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb
      };
      sentCommand.AddRange(FiscalPrinterCommands.SaveSettingCommand);
      sentCommand.AddRange(new[]
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb1,
        FiscalPrinterParameters.AbortSaveSetting,
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe
      });
      return sentCommand;
    }
  }
}