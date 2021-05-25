using System;
using System.Collections.Generic;
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
  public class MutatePercentageDiscountForPartialSumTests : FiscalPrinterCommandTestsBase
  {
    private readonly MutatePercentageDiscountForPartialSumRequest _sampleRequest = new()
    {
      Action = SimpleSumDiscountAction.Uplift,
      Percentage = 1
    };

    [Fact]
    public async Task MutatePercentageDiscountForPartialSumCommand_Works()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;

      var response = await  Run<MutatePercentageDiscountForPartialSumCommand, MutatePercentageDiscountForPartialSumRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      DSL.Assert(() => fullCommandString == expectedCommandString);
      DSL.Assert(() => response.Success);
    }

    [Fact]
    public async Task MutatePercentageDiscountForPartialSumCommand_Requires_Action()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Action));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Action));
    }

    [Fact]
    public async Task MutatePercentageDiscountForPartialSumCommand_Requires_Percentage_Greater_Than_0()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Percentage));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Percentage));
    }

    private List<byte> GetExpectedSentCommand(MutatePercentageDiscountForPartialSumRequest request)
    {
      byte[] EncodeWithTwoDecimalPositions(decimal value)
        => EncodingHelper.Encode(value.ToString("F"));
      
      var encodedPercentage = EncodeWithTwoDecimalPositions(request.Percentage);
      var encodedAction = request.Action.Encode(nameof(request.Action));

      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.F,
        encodedAction,
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb1
      };
      sentCommand.AddRange(encodedPercentage);

      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });
      return sentCommand;
    }

    private async Task AssertArgumentException<T>(MutatePercentageDiscountForPartialSumRequest request, string paramName)
      where T : ArgumentException
    {
      SetupAckRespondingPrinter();
      var exception = await Xunit.Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<MutatePercentageDiscountForPartialSumCommand, MutatePercentageDiscountForPartialSumRequest>(request);
      });
      DSL.Assert(() => exception.Message != null);
      DSL.Assert(() => exception.ParamName == paramName);
    }
  }
}