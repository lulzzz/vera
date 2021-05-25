using System.Threading.Tasks;
using Assertive;
using Vera.Poland.Commands.Settings;
using Xunit;

namespace Vera.Poland.Tests.Commands.Settings
{
  public class SaveSettingTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task SaveSettingCommand_Works()
    {
      SetupAckRespondingPrinter();
      var response = await  Run<SaveSettingCommand>();
      DSL.Assert(() => response.Success);
    }
  }
}