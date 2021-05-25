using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;
using Xunit;
using static Assertive.DSL;

namespace Vera.Poland.Tests.Commands.Invoice
{
  public class CancelReceiptOrInvoiceCommandTests : FiscalPrinterCommandTestsBase
  {
    private readonly CancelReceiptOrInvoiceRequest _sampleRequest = new()
    {
      Total = 1
    };

    [Fact]
    public async Task CancelReceiptOrInvoiceCommand_Works()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;

      var response = await  Run<CancelReceiptOrInvoiceCommand, CancelReceiptOrInvoiceRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      Assert(() => fullCommandString == expectedCommandString);
      Assert(() => response.Success);
    }

    [Fact]
    public async Task CancelReceiptOrInvoiceCommand_Throws_Exception_With_Negative_Total()
    {
      var request = _sampleRequest;
      request.Total = -1;
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Total));
    }

    private async Task AssertArgumentException<T>(CancelReceiptOrInvoiceRequest request, string paramName)
      where T : ArgumentException
    {
      SetupAckRespondingPrinter();

      var exception = await Xunit.Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<CancelReceiptOrInvoiceCommand, CancelReceiptOrInvoiceRequest>(request);
      });
      Assert(() => exception.Message != null);
      Assert(() => exception.ParamName == paramName);
    }

    private static List<byte> GetExpectedSentCommand(CancelReceiptOrInvoiceRequest request)
    {
      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.T, FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb1, FiscalPrinterDividers.c
      };

      var encodedTotal = EncodingHelper.Encode(request.Total);
      sentCommand.AddRange(encodedTotal);

      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });
      return sentCommand;
    }

  }
}