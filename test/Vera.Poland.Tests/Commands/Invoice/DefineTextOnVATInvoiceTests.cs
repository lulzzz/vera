using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vera.Extensions;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;
using Xunit;
using static Assertive.DSL;

namespace Vera.Poland.Tests.Commands.Invoice
{
  public class DefineTextOnVatInvoiceCommandTests : FiscalPrinterCommandTestsBase
  {
    private readonly DefineTextOnVatInvoiceRequest _sampleRequest = new()
    {
      Description = "test",
      TextLines = new List<string> { "test" }
    };

    [Fact]
    public async Task DefineTextOnVATInvoiceCommand_Works()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;
      await AssertSuccessfulCommand(request);
    }

    [Fact]
    public async Task DefineTextOnVATInvoiceCommand_Works_With_Multiple_TextLines()
    {
      SetupAckRespondingPrinter();
      var request = CloneExcludingProperty(_sampleRequest,nameof(_sampleRequest.TextLines));
      request.TextLines = new List<string> { "test 1", "test 2" };
      await AssertSuccessfulCommand(request);
    }

    [Fact]
    public async Task Will_Throw_Exception_With_Missing_Description()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Description));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Description));
    }

    [Fact]
    public async Task Will_Throw_Exception_With_Missing_TextLines()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.TextLines));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.TextLines));
    }

    [Fact]
    public async Task Will_Throw_Exception_When_More_Than_5_TextLines_Exist()
    {
      var request = _sampleRequest;
      request.TextLines = new List<string> { "test 1", "test 2", "test 3", "test 4", "test 5", "test 6" };
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.TextLines));
    }

    [Fact]
    public async Task Will_Throw_Exception_When_More_Than_13_Characters_Exist_On_One_TextLine()
    {
      var request = _sampleRequest;
      request.TextLines = new List<string> { "text_line_is_t" };
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.TextLines));
    }

    private async Task AssertSuccessfulCommand(DefineTextOnVatInvoiceRequest request)
    {
      var response = await  Run<DefineTextOnVatInvoiceCommand, DefineTextOnVatInvoiceRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      Assert(() => fullCommandString == expectedCommandString);
      Assert(() => response.Success);
    }

    private static List<byte> GetExpectedSentCommand(DefineTextOnVatInvoiceRequest request)
    {
      var encodedDescription= EncodingHelper.Encode(request.Description);

      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb,FiscalPrinterDividers.i, FiscalPrinterCommands.Esc
      };
      sentCommand.AddRange(encodedDescription);

      if (request.TextLines.Any())
      {
        foreach (var textLine in request.TextLines
          .Where(textLine => !textLine.IsNullOrWhiteSpace()))
        {
          var encodedLine = EncodingHelper.Encode(textLine);
          sentCommand.AddRange(new[] { FiscalPrinterDividers.Lf });
          sentCommand.AddRange(encodedLine);
        }
      }
      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });
      return sentCommand;
    }


    private async Task AssertArgumentException<T>(DefineTextOnVatInvoiceRequest request, string paramName)
      where T : ArgumentException
    {
      SetupAckRespondingPrinter();
      var exception = await Xunit.Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<DefineTextOnVatInvoiceCommand, DefineTextOnVatInvoiceRequest>(request);
      });
      Assert(() => exception.Message != null);
      Assert(() => exception.ParamName == paramName);
    }
  }
}