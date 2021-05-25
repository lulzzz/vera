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
  public class EndSaleTransactionCommandTest : FiscalPrinterCommandTestsBase
  {
    private const string DateFormat = "yyyy-MM-dd";
    private readonly EndSaleTransactionRequest _sampleRequest = new();

    [Fact]
    public async Task EndSaleTransactionCommand_Works()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;
      await AssertSuccessfulCommand(request);
    }

    [Fact]
    public async Task EndSaleTransactionCommand_Works_With_All_Parameters()
    {
      SetupAckRespondingPrinter();
      var request = new EndSaleTransactionRequest()
      {
        IsInvoice = true,
        SaleDate = DateTime.Now
      };
      await AssertSuccessfulCommand(request);
    }

    [Fact]
    public async Task DefineTextOnVATInvoiceCommand_Throws_Exception_With_NonInvoice()
    {
      var request = _sampleRequest;
      request.IsInvoice = false;
      request.SaleDate = DateTime.Now;
      await AssertInvalidOperationException(request);
    }

    private async Task AssertSuccessfulCommand(EndSaleTransactionRequest request)
    {
      var response = await  Run<EndSaleTransactionCommand, EndSaleTransactionRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      Assert(() => fullCommandString == expectedCommandString);
      Assert(() => response.Success);
    }

    private async Task AssertInvalidOperationException(EndSaleTransactionRequest request)
    {
      SetupAckRespondingPrinter();

      var exception = await Xunit.Assert.ThrowsAsync<InvalidOperationException>(async () =>
      {
        await  Run<EndSaleTransactionCommand, EndSaleTransactionRequest>(request);
      });
      Assert(() => exception.Message != null);
    }

    private static List<byte> GetExpectedSentCommand(EndSaleTransactionRequest request)
    {
      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.E
      };

      if (request.SaleDate.HasValue)
      {
        sentCommand.AddRange(EncodingHelper.ConvertDateToBytes(request.SaleDate.Value, DateFormat));
      }
      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });

      return sentCommand;
    }
  }
}