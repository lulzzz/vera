using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Poland.Commands;
using Vera.Poland.Models;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands
{
  public class OpenCashDrawerCommandTests : FiscalPrinterCommandTestsBase
  {
    [Theory]
    [InlineData(FiscalPrinterCashDrawer.DrawerOne, 1)]
    [InlineData(FiscalPrinterCashDrawer.DrawerTwo, 2)]
    public async Task Will_Send_Correct_Command_To_Device(FiscalPrinterCashDrawer drawer, int expectedCashDrawerNumber)
    {
      ResetPrinterWriteRawDataResponse();
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });

      var request = new OpenCashDrawerRequest
      {
        DrawerToOpen = drawer
      };

      var result = await  Run<OpenCashDrawerCommand, OpenCashDrawerRequest>(request);

      Assert.True(result.Success);

      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.X,
        FiscalPrinterDividers.Zero,
        FiscalPrinterDividers.Zero,
      };
      expectedCommand.AddRange(EncodingHelper.Encode(expectedCashDrawerNumber));
      expectedCommand.Add(FiscalPrinterCommands.Esc);
      expectedCommand.Add(FiscalPrinterCommands.Mfe);

      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());
      Assert.Equal(expectedCommandString, fullCommandString);
    }
  }
}