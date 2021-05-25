using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Poland.Commands.PredefinedReferencePrintouts;
using Vera.Poland.Models.Constants;
using Vera.Poland.Models.Requests.PredefinedReferencePrintouts;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.PredefinedReferencePrintouts
{
  public class PrintPredefinedLinesCommandTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task Will_Validate_Line()
    {
      var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
      {
        await  Run<PrintPredefinedLinesCommand, PrintPredefinedLinesRequest>(
          new PrintPredefinedLinesRequest
          {
            Line = null
          });
      });

      Assert.Equal(nameof(PrintPredefinedLinesRequest.Line), exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Will_Validate_PatternNumber(string patternNumber)
    {
      var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
      {
        await  Run<PrintPredefinedLinesCommand, PrintPredefinedLinesRequest>(
          new PrintPredefinedLinesRequest
          {
            PatternNumber = patternNumber,
            Line = "Test"
          });
      });

      Assert.Equal(nameof(PrintPredefinedLinesRequest.PatternNumber), exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Short_Param")]
    [InlineData("A_Very_Long_Parameter_Since_There_Is_No_Length_Check")]
    public async Task Will_Send_Correct_Data_To_Printer(string parameterValue)
    {
      ResetPrinterWriteRawDataResponse();
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });
      const string line = "50";

      var response = await  Run<PrintPredefinedLinesCommand, PrintPredefinedLinesRequest>(
        new PrintPredefinedLinesRequest
        {
          PatternNumber = PredefinedPrintoutPatternNumbers.ReturnReceipt,
          Line = line,
          ParameterValue = parameterValue
        });

      Assert.True(response.Success);

      GetExpectedCommand(line, parameterValue);
    }

    private void GetExpectedCommand(string line, string parameterValue)
    {
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.Z
      };
      expectedCommand.AddRange(EncodingHelper.Encode(PredefinedPrintoutPatternNumbers.ReturnReceipt));
      expectedCommand.AddRange(EncodingHelper.Encode(line));
      if (!string.IsNullOrWhiteSpace(parameterValue))
      {
        expectedCommand.AddRange(EncodingHelper.Encode(parameterValue));
      }
      expectedCommand.Add(FiscalPrinterCommands.Esc);
      expectedCommand.Add(FiscalPrinterCommands.Mfe);

      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());
      Assert.Equal(expectedCommandString, fullCommandString);
    }
  }
}