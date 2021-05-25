using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vera.Poland.Commands;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;
using Xunit;
using static Assertive.DSL;

namespace Vera.Poland.Tests.Commands
{
  public class ToggleSignatureCommandTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task Printer_Toggle_Signature_Error()
    {
      ResetPrinterWriteRawDataResponse();
      var request = new ToggleRequest
      {
        FeatureEnabled = true
      };

      var printerAckResponse = new[] { FiscalPrinterResponses.Nak };
      MockSinglePrinterResponse(printerAckResponse);

      var response = await  Run<ToggleSignatureCommand, ToggleRequest>(request);
      Assert(() => !response.Success);
    }

    [Fact]
    public async Task Enable_Signature_Success()
    {
      var request = new ToggleRequest
      {
        FeatureEnabled = true
      };

      await Printer_Toggle_Signature_Success(request);
    }

    [Fact]
    public async Task Disable_Signature_Success()
    {
      var request = new ToggleRequest
      {
        FeatureEnabled = false
      };

      await Printer_Toggle_Signature_Success(request);
    }

    private async Task Printer_Toggle_Signature_Success(ToggleRequest request)
    {
      ResetPrinterWriteRawDataResponse();
      var printerAckResponse = new[] { FiscalPrinterResponses.Ack };
      MockSinglePrinterResponse(printerAckResponse);

      var response = await  Run<ToggleSignatureCommand, ToggleRequest>(request);

      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      Assert(() => fullCommandString == expectedCommandString);
      Assert(() => response.Success);
    }

    private static IEnumerable<byte> GetExpectedSentCommand(ToggleRequest request)
    {
      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb
      };

      sentCommand.AddRange(FiscalPrinterDividers.Gp);
      sentCommand.AddRange(EncodingHelper.Encode(request.FeatureEnabled));
      sentCommand.AddRange(new []{ FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });

      return sentCommand;
    }
  }
}