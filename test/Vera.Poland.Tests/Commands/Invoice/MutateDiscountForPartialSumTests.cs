using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Extensions;
using Vera.Poland.Models;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;
using Xunit;
using static Assertive.DSL;

namespace Vera.Poland.Tests.Commands.Invoice
{
  public class MutateDiscountForPartialSumTests : FiscalPrinterCommandTestsBase
  {
    private readonly MutateDiscountForPartialSumRequest _sampleRequest = new()
    {
      Action = SimpleSumDiscountAction.Uplift,
      ActionType =  DiscountActionType.Give
    };

    [Fact]
    public async Task MutateDiscountForPartialSumCommand_Works()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;
      await TestSuccessfulCommand(request);
    }

    [Fact]
    public async Task MutateDiscountForPartialSumCommand_Requires_Action()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Action));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.Action));
    }

    [Fact]
    public async Task MutateDiscountForPartialSumCommand_Requires_ActionType()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.ActionType));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.ActionType));
    }

    private async Task AssertArgumentException<T>(MutateDiscountForPartialSumRequest request, string paramName)
      where T : ArgumentException
    {
      SetupAckRespondingPrinter();
      var exception = await Xunit.Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<MutateDiscountForPartialSumCommand, MutateDiscountForPartialSumRequest>(request);
      });

      Assert(() => exception.Message != null);
      Assert(() => exception.ParamName == paramName);
    }

    private async Task TestSuccessfulCommand(MutateDiscountForPartialSumRequest request)
    {
      var response =
        await  Run<MutateDiscountForPartialSumCommand, MutateDiscountForPartialSumRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      Assert(() => fullCommandString == expectedCommandString);
      Assert(() => response.Success);
    }

    private List<byte> GetExpectedSentCommand(MutateDiscountForPartialSumRequest request)
    {

      byte[] EncodeAmounts(VatAmounts amounts)
      {
        // ensure it is ordered
        var orderedAmounts = amounts.AmountsInRates
          .OrderBy(amount => amount.Vat).ToList();

        var bytes = new List<byte>();

        foreach (var amountStringRepresentation in
          orderedAmounts.Select(orderedAmount => orderedAmount.Amount.ToString("F")))
        {
          bytes.AddRange(EncodingHelper.Encode(amountStringRepresentation));
          bytes.Add(FiscalPrinterDividers.Lf);
        }

        return bytes.ToArray();
      }

      var encodedDiscountAction = request.Action.Encode(nameof(request.Action));
      var encodedDiscountActionType = request.ActionType.Encode();
      var encodedValue = EncodingHelper.Encode(request.Value);


      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.f,
        encodedDiscountAction,
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb1
      };
      
      if (request.Amounts != null)
      {
        var encodedAmounts = EncodeAmounts(request.Amounts);
        sentCommand.AddRange(encodedAmounts);
        sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb2 });
      }

      sentCommand.AddRange(new[] { encodedDiscountActionType});
      sentCommand.AddRange(encodedValue);

      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });
      return sentCommand;
    }
  }
}