using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assertive;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.Invoice
{
  public class PrintVatRegistrationNumberTests : FiscalPrinterCommandTestsBase
  {
    private readonly PrintVatRegistrationNumberRequest _sampleRequest = new()
    {
      CustomerNip = "CustomerNIP"
    };

    [Fact]
    public async Task PrintVATRegistrationNumberCommand_Works()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;

      var response = await  Run<PrintVatRegistrationNumberCommand, PrintVatRegistrationNumberRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      DSL.Assert(() => fullCommandString == expectedCommandString);
      DSL.Assert(() => response.Success);
    }

    [Fact]
    public async Task PrintVATRegistrationNumberCommand_Requires_CustomerNIP()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.CustomerNip));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.CustomerNip));
    }

    private List<byte> GetExpectedSentCommand(PrintVatRegistrationNumberRequest request)
    {
      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.j
      };
      sentCommand.AddRange(EncodingHelper.Encode(request.CustomerNip));
      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });
      return sentCommand;
    }

    private async Task AssertArgumentException<T>(PrintVatRegistrationNumberRequest request, string paramName)
      where T : ArgumentException
    {
      SetupAckRespondingPrinter();
      var exception = await Xunit.Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<PrintVatRegistrationNumberCommand, PrintVatRegistrationNumberRequest>(request);
      });
      DSL.Assert(() => exception.Message != null);
      DSL.Assert(() => exception.ParamName == paramName);
    }
  }
}