using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Poland.Commands.ProtectedMemory;
using Vera.Poland.Models.Requests.ProtectedMemory;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.ProtectedMemory
{
  public class PrintDocumentCommandTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task Will_Send_Correct_Command_To_Printer()
    {
      const uint jpkid = 1243;
      ResetPrinterWriteRawDataResponse();
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });

      var response = await  Run<PrintDocumentCommand, PrintDocumentRequest>(new PrintDocumentRequest
      {
        JPKID = jpkid
      });

      Assert.True(response.Success);

      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.H,
        FiscalPrinterDividers.E, FiscalPrinterDividers.J,
      };
      expectedCommand.AddRange(EncodingHelper.Encode(jpkid));
      expectedCommand.Add(FiscalPrinterCommands.Esc);
      expectedCommand.Add(FiscalPrinterCommands.Mfe);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      Assert.Equal(expectedCommandString, fullCommandString);
    }
  }
}