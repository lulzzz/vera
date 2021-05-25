using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assertive;
using Vera.Extensions;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.Invoice
{
  public class OpenInvoiceTests : FiscalPrinterCommandTestsBase
  {
    private readonly OpenInvoiceRequest _sampleRequest = new();

    [Fact]
    public async Task OpenInvoiceCommand_Works()
    {
      var request = _sampleRequest;
      SetupAckRespondingPrinter();
      await TestSuccessfulCommand(request);
    }

    [Fact]
    public async Task OpenInvoiceCommand_Works_With_Parameter()
    {
      var request = new OpenInvoiceRequest
      {
        InvoiceIdentifier = "Identifier"
      };
      SetupAckRespondingPrinter();
      await TestSuccessfulCommand(request);
    }

    [Fact]
    public async Task OpenInvoiceCommand_InvoiceIdentifier_Has_Max_Length()
    {
      var request = new OpenInvoiceRequest()
      {
        InvoiceIdentifier = Faker.Random.AlphaNumeric(OpenInvoiceCommand.MaxInvoiceCharacters + 1)
      };

      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.InvoiceIdentifier));
    }

    private async Task TestSuccessfulCommand(OpenInvoiceRequest request)
    {
      var response = await  Run<OpenInvoiceCommand, OpenInvoiceRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      DSL.Assert(() => fullCommandString == expectedCommandString);
      DSL.Assert(() => response.Success);
    }

    private List<byte> GetExpectedSentCommand(OpenInvoiceRequest request)
    {
      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.C
      };

      if (!request.InvoiceIdentifier.IsNullOrWhiteSpace())
      {
        var encodedInvoice = EncodingHelper.Encode(request.InvoiceIdentifier);
        sentCommand.AddRange(encodedInvoice);
      }
      
      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });
      return sentCommand;
    }

    private async Task AssertArgumentException<T>(OpenInvoiceRequest request, string paramName)
      where T : ArgumentException
    {
      var exception = await Xunit.Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<OpenInvoiceCommand, OpenInvoiceRequest>(request);
      });
      DSL.Assert(() => exception.Message != null);
      DSL.Assert(() => exception.ParamName == paramName);
    }
  }
}