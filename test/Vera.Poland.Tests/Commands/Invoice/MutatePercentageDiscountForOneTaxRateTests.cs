using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assertive;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.Invoice
{
  public class MutatePercentageDiscountForOneTaxRateTests : FiscalPrinterCommandTestsBase
  {
    private readonly MutatePercentageDiscountForOneTaxRateRequest _sampleRequest = new()
    {
      Action = SimpleSumDiscountAction.Uplift,
      Type = DiscountType.Percentage,
      Vat = VatClass.A,
      Value = 1
    };

    [Fact]
    public async Task MutatePercentageDiscountForOneTaxRateCommand_Works()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;

      var response = await  Run<MutatePercentageDiscountForOneTaxRateCommand, MutatePercentageDiscountForOneTaxRateRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      DSL.Assert(() => fullCommandString == expectedCommandString);
      DSL.Assert(() => response.Success);
    }

    [Fact]
    public async Task MutatePercentageDiscountForOneTaxRateCommand_Requires_Action()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Action));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.Action));
    }

    [Fact]
    public async Task MutatePercentageDiscountForOneTaxRateCommand_Requires_Type()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Type));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.Type));
    }

    [Fact]
    public async Task MutatePercentageDiscountForOneTaxRateCommand_Requires_Vat()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Vat));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.Vat));
    }

    [Fact]
    public async Task MutatePercentageDiscountForOneTaxRateCommand_Requires_Value_Above_0()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Value));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Value));
    }

    private List<byte> GetExpectedSentCommand(MutatePercentageDiscountForOneTaxRateRequest request)
    {
      static byte EncodeDiscountType(DiscountType discountType)
      {
        return discountType switch
        {
          DiscountType.Amount => FiscalPrinterDividers.v,
          DiscountType.Percentage => FiscalPrinterDividers.V,
          _ => throw new ArgumentOutOfRangeException(nameof(discountType), discountType, null)
        };
      }

      var encodedDiscountType = EncodeDiscountType(request.Type);
      var encodedDiscountAction = request.Action.Encode(nameof(request.Action));
      var encodedVat = request.Vat.EncodeVatClass();
      var encodedValue = request.Value.ToFixedPointByteArray();

      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        encodedDiscountType,
        encodedDiscountAction,
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb1
      };
      sentCommand.AddRange(encodedValue.ToArray());
      sentCommand.AddRange(new[] {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb2,
        encodedVat,
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfe});

      return sentCommand;
    }

    private async Task AssertArgumentException<T>(MutatePercentageDiscountForOneTaxRateRequest request, string paramName)
      where T : ArgumentException
    {
      SetupAckRespondingPrinter();
      var exception = await Xunit.Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<MutatePercentageDiscountForOneTaxRateCommand, MutatePercentageDiscountForOneTaxRateRequest>(request);
      });
      DSL.Assert(() => exception.Message != null);
      DSL.Assert(() => exception.ParamName == paramName);
    }
  }
}