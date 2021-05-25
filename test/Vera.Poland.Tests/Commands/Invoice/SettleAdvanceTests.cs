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
{ public class SettleAdvanceTests : FiscalPrinterCommandTestsBase
  {

    private readonly SettleAdvanceRequest _sampleRequest = new()
    {
      ProductName= "ProductName",
      Quantity = 10,
      Price = 10,
      Value = 10,
      Vat =  VatClass.A,
      ToDocument = "ToDocument",
      SettlementOfTheAdvanceComment = "settlement of the advance",
      AdvanceAmount = 10
    };

    [Fact]
    public async Task SettleAdvanceCommand_Works()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;

      var response = await  Run<SettleAdvanceCommand, SettleAdvanceRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      Assert(() => fullCommandString == expectedCommandString);
      Assert(() => response.Success);
    }

    [Fact]
    public async Task SettleAdvanceCommand_Works_With_All_Fields()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;
      request.SettlementOfTheAdvanceComment = "SettlementOfTheAdvanceComment";
      request.SupplementAmount = 10;

      var response = await  Run<SettleAdvanceCommand, SettleAdvanceRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      Assert(() => fullCommandString == expectedCommandString);
      Assert(() => response.Success);
    }

    [Fact]
    public async Task SettleAdvanceCommand_Requires_ProductName()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.ProductName));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.ProductName));
    }

    [Fact]
    public async Task SettleAdvanceCommand_Requires_Vat()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Vat));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.Vat));
    }

    [Fact]
    public async Task SettleAdvanceCommand_Requires_ToDocument()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.ToDocument));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.ToDocument));
    }

    [Fact]
    public async Task SettleAdvanceCommand_Requires_AdvanceAmount()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.AdvanceAmount));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.AdvanceAmount));
    }

    [Fact]
    public async Task SettleAdvanceCommand_Requires_Quantity()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Quantity));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Quantity));
    }

    [Fact]
    public async Task SettleAdvanceCommand_Requires_ValidQuantity()
    {
      var request = _sampleRequest;
      request.Quantity = -10;
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Quantity));
    }

    [Fact]
    public async Task SettleAdvanceCommand_Requires_Price()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Price));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Price));
    }

    [Fact]
    public async Task SettleAdvanceCommand_Requires_ValidPrice()
    {
      var request = _sampleRequest;
      request.Price = -10;
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Price));
    }

    [Fact]
    public async Task SettleAdvanceCommand_Requires_Value()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Value));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Value));
    }

    [Fact]
    public async Task SettleAdvanceCommand_Requires_ValidValue()
    {
      var request = _sampleRequest;
      request.Value = -10;
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Value));
    }

    private List<byte> GetExpectedSentCommand(SettleAdvanceRequest request)
    {
      var encodedProductName = EncodingHelper.Encode(request.ProductName);
      var encodedQuantity = EncodingHelper.EncodeQuantity(request.Quantity);
      var encodedPrice = EncodingHelper.Encode(request.Price);
      var encodedValue = EncodingHelper.Encode(request.Value);
      var encodedVatClass = request.Vat.EncodeVatClass();
      var encodedToDocument = EncodingHelper.Encode(request.ToDocument);
      var encodedAdvanceAmount = EncodingHelper.Encode(request.AdvanceAmount);
      var encodedSupplementAmount = EncodingHelper.Encode(request.SupplementAmount);
      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.D
      };
      sentCommand.AddRange(encodedProductName);
      sentCommand.AddRange(new[] { FiscalPrinterDividers.Lf });
      sentCommand.AddRange(encodedQuantity);
      sentCommand.AddRange(new[] { FiscalPrinterDividers.Star });
      sentCommand.AddRange(encodedPrice);
      sentCommand.AddRange(new[] { FiscalPrinterDividers.Lf, FiscalPrinterDividers.Cr });
      sentCommand.AddRange(EncodingHelper.Encode(request.SettlementOfTheAdvanceComment));

      sentCommand.AddRange(encodedValue);
      sentCommand.AddRange(new[] { FiscalPrinterDividers.Lf, FiscalPrinterDividers.Cr });
      sentCommand.AddRange(encodedToDocument);
      sentCommand.AddRange(new[] { FiscalPrinterDividers.Lf, FiscalPrinterDividers.Cr });
      sentCommand.AddRange(encodedAdvanceAmount);
      sentCommand.AddRange(new[] { FiscalPrinterDividers.Sp });
      sentCommand.AddRange(new[] { encodedVatClass });

      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb1, FiscalPrinterDividers.a });
      sentCommand.AddRange(encodedSupplementAmount);
      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb2 });
      sentCommand.AddRange(new[] { encodedVatClass });

      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });

      return sentCommand;
    }

    private async Task AssertArgumentException<T>(SettleAdvanceRequest request, string paramName)
      where T : ArgumentException
    {
      SetupAckRespondingPrinter();
      var exception = await Xunit.Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<SettleAdvanceCommand, SettleAdvanceRequest>(request);
      });
      Assert(() => exception.Message != null);
      Assert(() => exception.ParamName == paramName);
    }
  }
}