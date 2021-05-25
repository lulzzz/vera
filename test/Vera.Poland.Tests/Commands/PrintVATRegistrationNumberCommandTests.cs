using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands
{
  public class PrintVatRegistrationNumberCommandTests : FiscalPrinterCommandTestsBase
  {
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task When_CustomerNIP_Is_Invalid_Will_Throw(string customerNip)
    {
      ResetPrinterWriteRawDataResponse();
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });

      var request = new PrintVatRegistrationNumberRequest
      {
        CustomerNip = customerNip
      };

      var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
      {
        await  Run<PrintVatRegistrationNumberCommand, PrintVatRegistrationNumberRequest>(request);
      });

      Assert.Equal(nameof(PrintVatRegistrationNumberRequest.CustomerNip), exception.ParamName);
    }

    [Fact]
    public async Task Will_Send_Correct_Command_To_Printer()
    {
      ResetPrinterWriteRawDataResponse();
      MockExactPrinterResponse(new []{FiscalPrinterResponses.Ack});
      const string customerNip = "201399-21938-12312";

      var request = new PrintVatRegistrationNumberRequest
      {
        CustomerNip = customerNip
      };

      var response = await  Run<PrintVatRegistrationNumberCommand, PrintVatRegistrationNumberRequest>(request);

      Assert.True(response.Success);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.j
      };

      expectedCommand.AddRange(EncodingHelper.Encode(customerNip));
      expectedCommand.Add(FiscalPrinterCommands.Esc);
      expectedCommand.Add(FiscalPrinterCommands.Mfe);

      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());
      Assert.Equal(expectedCommandString, fullCommandString);
    }
  }
}