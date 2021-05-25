using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assertive;
using Vera.Poland.Commands;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands
{
  public class SetDateCommandTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task Printer_Set_Date_Success()
    {
      var testDate = new DateTime(2021, 02, 03, 18, 33, 19);
      const string dateFormat = "ss,mm,HH;dd,MM,yy";
      var encodedTestDate = EncodingHelper.ConvertDateToBytes(testDate, dateFormat);

      ResetPrinterWriteRawDataResponse();
      var printerAckResponse = new[] { FiscalPrinterResponses.Ack };
      MockSinglePrinterResponse(printerAckResponse);
      var request = new SetDateRequest
      {
        Date = testDate
      };

      var response = await  Run<SetDateCommand, SetDateRequest>(request);

      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(encodedTestDate);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      DSL.Assert(() => fullCommandString == expectedCommandString);
      DSL.Assert(() => response.Success);
    }

    private static IEnumerable<byte> GetExpectedSentCommand(byte[] encodedTestDate)
    {
      var commandStartPart = new[]
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.I
      };

      var commandEndPart = new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe };

      var expectedSentCommand = commandStartPart.Concat(encodedTestDate).Concat(commandEndPart).ToArray();
      return expectedSentCommand;
    }
  }
}