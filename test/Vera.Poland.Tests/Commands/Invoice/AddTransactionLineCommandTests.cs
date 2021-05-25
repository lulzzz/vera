using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Extensions;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;
using Xunit;
using static Assertive.DSL;

namespace Vera.Poland.Tests.Commands.Invoice
{
  public class AddTransactionLineCommandTest : FiscalPrinterCommandTestsBase
  {
    private readonly AddTransactionLineRequest _sampleRequest = new()
    {
      ProductName = "ProductName",
      Quantity = 1,
      Price = 1,
      Value = 429496.72M,
      Unit = "Unit",
      Vat = VatClass.A
    };

    [Fact]
    public async Task AddTransactionLineCommand_Works()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;

      var response = await  Run<AddTransactionLineCommand, AddTransactionLineRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      Assert(() => fullCommandString == expectedCommandString);
      Assert(() => response.Success);
    }

    [Fact]
    public async Task AddTransactionLineCommand_Throws_Exception_With_Invalid_Quantity()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Quantity));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Quantity));
    }

    [Fact]
    public async Task AddTransactionLineCommand_Throws_Exception_With_Invalid_Price()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Price));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Price));
    }

    [Fact]
    public async Task AddTransactionLineCommand_Throws_Exception_With_Invalid_Value()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Value));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Value));
    }

    [Fact]
    public async Task AddTransactionLineCommand_Throws_Exception_With_Invalid_Vat()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Vat));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.Vat));
    }

    private async Task AssertArgumentException<T>(AddTransactionLineRequest request, string paramName)
      where T : ArgumentException
    {
      SetupAckRespondingPrinter();

      var exception = await Xunit.Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<AddTransactionLineCommand, AddTransactionLineRequest>(request);
      });
      Assert(() => exception.Message != null);
      Assert(() => exception.ParamName == paramName);
    }

    private List<byte> GetExpectedSentCommand(AddTransactionLineRequest request)
    {
      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.D
      };

      var encodedProductName = EncodingHelper.Encode(request.ProductName);
      var encodedQuantity = EncodingHelper.EncodeQuantity(request.Quantity);
      var encodedPrice = EncodingHelper.Encode(request.Price);
      var encodedValue = EncodingHelper.Encode(request.Value);
      var encodedVatClass = request.Vat.EncodeVatClass();

      sentCommand.AddRange(encodedProductName);
      sentCommand.AddRange(new[] { FiscalPrinterDividers.Nul });
      sentCommand.AddRange(encodedQuantity);

      if (!request.Unit.IsNullOrWhiteSpace())
      {
        var encodedUnit = EncodingHelper.Encode(request.Unit);
        sentCommand.AddRange(encodedUnit);
      }
      sentCommand.AddRange(new[] { FiscalPrinterDividers.Star });
      sentCommand.AddRange(encodedPrice);

      MaybeWriteComment(sentCommand, request.Comment1);
      MaybeWriteComment(sentCommand, request.Comment2);

      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb1, FiscalPrinterDividers.a });
      sentCommand.AddRange(encodedValue);

      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb2 });
      sentCommand.AddRange(new[] { encodedVatClass });

      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });
      return sentCommand;
    }
  }
}