using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;
using Xunit;
using static Assertive.DSL;

namespace Vera.Poland.Tests.Commands.Invoice
{
  public class CancelTransactionLineCommandTests : FiscalPrinterCommandTestsBase
  {
    private readonly CancelTransactionLineRequest _sampleRequest = new()
    {
      ProductName = "ProductName",
      Quantity = 1,
      Price = 1,
      Value = 429496.72M,
      Vat = VatClass.A
    };

    [Fact]
    public async Task CancelTransactionLineCommand_Works()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;
      await TestSuccessfulCommand(request);
    }

    [Fact]
    public async Task CancelTransactionLineCommand_Works_With_All_Params()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;
      request.Comment1 = "comment 1";
      request.Comment2 = "comment 2";
      await TestSuccessfulCommand(request);
    }

    [Fact]
    public async Task CancelTransactionLineCommand_Throws_Exception_With_Invalid_Quantity()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Quantity));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Quantity));
    }

    [Fact]
    public async Task CancelTransactionLineCommand_Throws_Exception_With_Invalid_ProductName()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.ProductName));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.ProductName));
    }

    [Fact]
    public async Task CancelTransactionLineCommand_Throws_Exception_With_Invalid_Price()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Price));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Price));
    }

    [Fact]
    public async Task CancelTransactionLineCommand_Throws_Exception_With_Invalid_Value()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Value));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Value));
    }

    [Fact]
    public async Task CancelTransactionLineCommand_Throws_Exception_With_Invalid_Vat()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Vat));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.Vat));
    }

    private async Task TestSuccessfulCommand(CancelTransactionLineRequest request)
    {
      var response = await  Run<CancelTransactionLineCommand, CancelTransactionLineRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      Assert(() => fullCommandString == expectedCommandString);
      Assert(() => response.Success);
    }

    private List<byte> GetExpectedSentCommand(CancelTransactionLineRequest request)
    {
      var encodedProductName = EncodingHelper.Encode(request.ProductName);
      var encodedQuantity = EncodingHelper.EncodeQuantity(request.Quantity);
      var encodedPrice = EncodingHelper.Encode(request.Price);
      var encodedValue = EncodingHelper.Encode(request.Value);
      var encodedVatClass = request.Vat.EncodeVatClass();

      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.D
      };
      sentCommand.AddRange(encodedProductName);
      sentCommand.AddRange(new[] { FiscalPrinterDividers.Nul });

      sentCommand.AddRange(encodedQuantity);
      sentCommand.AddRange(new[] { FiscalPrinterDividers.Star });
      sentCommand.AddRange(encodedPrice);

      MaybeWriteComment(sentCommand, request.Comment1);
      MaybeWriteComment(sentCommand, request.Comment2);

      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb1, FiscalPrinterDividers.c, FiscalPrinterDividers.Minus });

      sentCommand.AddRange(encodedValue);
      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb2 });
      sentCommand.AddRange(new[] { encodedVatClass});
      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });
      
      return sentCommand;
    }

    private async Task AssertArgumentException<T>(CancelTransactionLineRequest request, string paramName)
      where T: ArgumentException
    {
      SetupAckRespondingPrinter();
      var exception = await Xunit.Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<CancelTransactionLineCommand, CancelTransactionLineRequest>(request);
      });
      Assert(() => exception.Message != null);
      Assert(() => exception.ParamName == paramName);
    }
  }
}