using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assertive;
using Vera.Extensions;
using Vera.Poland.Commands;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;
using Xunit;
using Assert = Xunit.Assert;

namespace Vera.Poland.Tests.Commands
{
  public class SetLineDisplayCommandTests : FiscalPrinterCommandTestsBase
  {

    [Fact]
    public async Task Printer_Set_Line_Display_Error()
    {
      ResetPrinterWriteRawDataResponse();
      var request = new SetLineDisplayRequest
      {
        Text = "bla", Type = LineDisplayType.FirstLine
      };

      var printerAckResponse = new[] { FiscalPrinterResponses.Nak };
      MockSinglePrinterResponse(printerAckResponse);

      var response = await  Run<SetLineDisplayCommand, SetLineDisplayRequest>(request);

      DSL.Assert(() => !response.Success);
    }

    [Fact]
    public async Task Printer_Set_Line_Display_Unspecified_Type_Error()
    {
      ResetPrinterWriteRawDataResponse();
      var request = new SetLineDisplayRequest
      {
        Text = "bla"
      };

      var printerAckResponse = new[] { FiscalPrinterResponses.Nak };
      MockSinglePrinterResponse(printerAckResponse);

      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<SetLineDisplayCommand, SetLineDisplayRequest>(request);
      });

      Assert.Equal(nameof(request.Type), exception.ParamName);
    }

    [Fact]
    public async Task Printer_Set_Line_Display_Lengthy_Text_Error()
    {
      ResetPrinterWriteRawDataResponse();
      var request = new SetLineDisplayRequest
      {
        Text = Enumerable.Repeat("a", 21).Aggregate(string.Concat),
        Type = LineDisplayType.FirstLine
      };

      var printerAckResponse = new[] { FiscalPrinterResponses.Nak };
      MockSinglePrinterResponse(printerAckResponse);

      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<SetLineDisplayCommand, SetLineDisplayRequest>(request);
      });

      Assert.Equal(nameof(request.Text), exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Welcome")]
    public async Task TestSuccess(string text)
    {
      ResetPrinterWriteRawDataResponse();
      var printerAckResponse = new[] { FiscalPrinterResponses.Ack };
      MockSinglePrinterResponse(printerAckResponse);

      var request = new SetLineDisplayRequest
      {
        Text = text,
        Type = LineDisplayType.FirstLine
      };

      var response = await  Run<SetLineDisplayCommand, SetLineDisplayRequest>(request);

      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      DSL.Assert(() => fullCommandString == expectedCommandString);
      DSL.Assert(() => response.Success);
    }

    private static IEnumerable<byte> GetExpectedSentCommand(SetLineDisplayRequest request)
    {
      byte lineIdentifier = request.Type switch
      {
        LineDisplayType.FirstLine => 0x30,
        LineDisplayType.SecondLine => 0x31,
        _ => throw new ArgumentOutOfRangeException(nameof(request.Type))
      };

      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.G,

        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb1,

        lineIdentifier
      };

      sentCommand.AddRange(request.Text.IsNullOrEmpty()
        ? EncodingHelper.Encode(Enumerable.Repeat(" ", 20).Aggregate(string.Concat))
        : EncodingHelper.Encode(request.Text));

      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });

      return sentCommand;
    }
  }
}