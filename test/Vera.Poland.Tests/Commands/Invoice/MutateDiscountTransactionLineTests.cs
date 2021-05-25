using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Models.Responses;
using Vera.Poland.Protocol;
using Xunit;
using static Assertive.DSL;

namespace Vera.Poland.Tests.Commands.Invoice
{
  public class MutateDiscountTransactionLineTests : FiscalPrinterCommandTestsBase
  {
    private readonly MutateDiscountTransactionLineRequest _sampleRequest = new()
    {
      Action = DiscountAction.Discount,
      ActionType =  DiscountActionType.Give,
      ProductName = "ProductName",
      Type = DiscountType.Percentage,
      Vat = VatClass.A,
      Value = 1
    };

    [Fact]
    public async Task MutateDiscountTransactionLineCommand_Works()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;

      var response = await  Run<MutateDiscountTransactionLineCommand, MutateDiscountTransactionLineRequest>(request);
      TestSuccessfulCommand(request, response);
    }

    [Fact]
    public async Task MutateDiscountTransactionLineCommand_Works_With_Percentages()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;
      request.ValueInPercentage = 1;
      var response = await  Run<MutateDiscountTransactionLineCommand, MutateDiscountTransactionLineRequest>(request);
      TestSuccessfulCommand(request, response);
    }

    [Fact]
    public async Task MutateDiscountTransactionLineCommand_Requires_ProductName()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.ProductName));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.ProductName));
    }

    [Fact]
    public async Task MutateDiscountTransactionLineCommand_Requires_Action()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Action));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.Action));
    }

    [Fact]
    public async Task MutateDiscountTransactionLineCommand_Requires_ActionType()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.ActionType));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.ActionType));
    }

    [Fact]
    public async Task MutateDiscountTransactionLineCommand_Requires_Type()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Type));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.Type));
    }

    [Fact]
    public async Task MutateDiscountTransactionLineCommand_Requires_Vat()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Vat));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.Vat));
    }

    [Fact]
    public async Task MutateDiscountTransactionLineCommand_Requires_Value_Above_0()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Value));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Value));
    }

    private void TestSuccessfulCommand(MutateDiscountTransactionLineRequest request, PrinterResponse response)
    {
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      Assert(() => fullCommandString == expectedCommandString);
      Assert(() => response.Success);
    }

    private List<byte> GetExpectedSentCommand(MutateDiscountTransactionLineRequest request)
    {
      var encodedProductName = EncodingHelper.Encode(request.ProductName);
      var encodedVatClass = request.Vat.EncodeVatClass();
      var encodedDiscountAction = request.Action.Encode();
      var encodedDiscountType = request.Type.Encode();
      var encodedDiscountActionType = request.ActionType.Encode();
      var encodedValue = EncodingHelper.Encode(request.Value);


      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.d
      };

      sentCommand.AddRange(encodedProductName);
      sentCommand.AddRange(new[] { FiscalPrinterDividers.Nul,
        encodedDiscountActionType,
        encodedDiscountType,
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb1,
        encodedDiscountAction
      });
      sentCommand.AddRange(encodedValue);
      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb2, encodedVatClass });

      if (request.ValueInPercentage.HasValue)
      {
        var encodedValueInPercentage = EncodingHelper.Encode(request.ValueInPercentage.Value);
        sentCommand.AddRange(encodedValueInPercentage);
      }
      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });

      return sentCommand;
    }

    private async Task AssertArgumentException<T>(MutateDiscountTransactionLineRequest request, string paramName)
      where T : ArgumentException
    {
      SetupAckRespondingPrinter();
      var exception = await Xunit.Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<MutateDiscountTransactionLineCommand, MutateDiscountTransactionLineRequest>(request);
      });

      Assert(() => exception.Message != null);
      Assert(() => exception.ParamName == paramName);
    }
  }
}