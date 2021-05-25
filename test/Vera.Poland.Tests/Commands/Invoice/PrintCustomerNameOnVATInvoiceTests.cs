using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assertive;
using Vera.Extensions;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.Invoice
{
  public class PrintCustomerNameOnVatInvoiceTests : FiscalPrinterCommandTestsBase
  {
    private readonly PrintCustomerNameOnVatInvoiceRequest _sampleRequest = new()
    {
      CustomerNameLines = new List<string>
      {
        "John Doe"
      }
    };

    [Fact]
    public async Task PrintCustomerNameOnVatInvoiceCommand_Works()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;

      var response =
        await  Run<PrintCustomerNameOnVatInvoiceCommand, PrintCustomerNameOnVatInvoiceRequest>(request);

      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      DSL.Assert(() => fullCommandString == expectedCommandString);
      DSL.Assert(() => response.Success);
    }

    [Fact]
    public async Task PrintCustomerNameOnVatInvoiceCommand_Requires_Customer_Lines()
    {
      var request = CloneExcludingProperty(_sampleRequest,nameof(_sampleRequest.CustomerNameLines));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.CustomerNameLines));
    }

    [Fact]
    public async Task PrintCustomerNameOnVatInvoiceCommand_Has_Max_Number_Of_Customer_Lines()
    {
      var request = _sampleRequest;
      for (var i = 0; i < PrintCustomerNameOnVatInvoiceCommand.MaxNumberOfCustomers; i++)
      {
        request.CustomerNameLines.Add(Faker.Random.AlphaNumeric(PrintCustomerNameOnVatInvoiceCommand.MaxCharactersInLine / 2));
      }
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.CustomerNameLines));
    }

    [Fact]
    public async Task PrintCustomerNameOnVatInvoiceCommand_Customers_Have_Max_Length()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.CustomerNameLines));
      request.CustomerNameLines = new()
      {
        Faker.Random.AlphaNumeric(PrintCustomerNameOnVatInvoiceCommand.MaxCharactersInLine + 1)
      };
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.CustomerNameLines));
    }

    private List<byte> GetExpectedSentCommand(PrintCustomerNameOnVatInvoiceRequest request)
    {
      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.i
      };

      for (var i = 0; i < request.CustomerNameLines.Count; i++)
      {
        if (request.CustomerNameLines[i].IsNullOrWhiteSpace())
        {
          continue;
        }
        sentCommand.AddRange(EncodingHelper.Encode(request.CustomerNameLines[i]));

        if (i + 1 != request.CustomerNameLines.Count)
        {
          sentCommand.AddRange(new[] { FiscalPrinterDividers.Lf });
        }
      }
      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });
      return sentCommand;
    }

    private async Task AssertArgumentException<T>(PrintCustomerNameOnVatInvoiceRequest request, string paramName)
      where T : ArgumentException
    {
      SetupAckRespondingPrinter();

      var exception = await Xunit.Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<PrintCustomerNameOnVatInvoiceCommand, PrintCustomerNameOnVatInvoiceRequest>(request);
      });

      DSL.Assert(() => exception.Message != null);
      DSL.Assert(() => exception.ParamName == paramName);
    }
  }
}