using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assertive;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.Invoice
{ public class PrintSumOfTransactionTests : FiscalPrinterCommandTestsBase
  {

    private readonly PrintSumOfTransactionRequest _sampleRequest = new()
    {
      Total = 10
    };

    [Fact]
    public async Task PrintSumOfTransactionCommand_Works()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;

      var response = await  Run<PrintSumOfTransactionCommand, PrintSumOfTransactionRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      DSL.Assert(() => fullCommandString == expectedCommandString);
      DSL.Assert(() => response.Success);
    }

    [Fact]
    public async Task PrintSumOfTransactionCommand_Requires_Total()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Total));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Total));
    }

    private static List<byte> GetExpectedSentCommand(PrintSumOfTransactionRequest request)
    {
      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.T,
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb1,
        FiscalPrinterDividers.a
      };
      sentCommand.AddRange(EncodingHelper.Encode(request.Total));
      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });
      return sentCommand;
    }

    private async Task AssertArgumentException<T>(PrintSumOfTransactionRequest request, string paramName)
      where T : ArgumentException
    {
      SetupAckRespondingPrinter();
      var exception = await Xunit.Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<PrintSumOfTransactionCommand, PrintSumOfTransactionRequest>(request);
      });
      DSL.Assert(() => exception.Message != null);
      DSL.Assert(() => exception.ParamName == paramName);
    }
  }
}