using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.Invoice
{
  public class PrintFooterPaymentDetailsCommandTests : FiscalPrinterCommandTestsBase
  {
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Will_Validate_TextParameter(string textParameter)
    {
      var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
      {
        await  Run<PrintFooterPaymentDetailsCommand, PrintFooterPaymentDetailsRequest>(
          BuildRequest(textParameter: textParameter));
      });

      Assert.Equal(nameof(PrintFooterPaymentDetailsRequest.TextParameter), exception.ParamName);
    }
    
    [Fact]
    public async Task Will_Send_Correct_Command_To_The_Printer()
    {
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });
      var request = BuildRequest();
      var result = await  Run<PrintFooterPaymentDetailsCommand, PrintFooterPaymentDetailsRequest>(request);

      Assert.True(result.Success);

      GetExpectedCommand(request);
    }

    private void GetExpectedCommand(PrintFooterPaymentDetailsRequest request)
    {
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.R,
      };
      expectedCommand.AddRange(EncodingHelper.Encode(SupportedPaymentAndFooterTypes.h.ToString()));
      expectedCommand.AddRange(EncodingHelper.Encode(request.TextParameter));
      expectedCommand.Add(FiscalPrinterDividers.Lf);
      expectedCommand.Add(FiscalPrinterDividers.C);
      expectedCommand.Add(FiscalPrinterDividers.Lf);
      expectedCommand.Add(FiscalPrinterDividers.Zero);

      expectedCommand.Add(FiscalPrinterCommands.Esc);
      expectedCommand.Add(FiscalPrinterCommands.Mfe);

      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());
      Assert.Equal(expectedCommandString, fullCommandString);
    }

    private static PrintFooterPaymentDetailsRequest BuildRequest(string textParameter = "TestTextParameter")
    {
      return new()
      {
        TextParameter = textParameter
      };
    }
  }
}