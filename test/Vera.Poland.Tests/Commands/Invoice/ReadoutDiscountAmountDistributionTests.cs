using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assertive;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Models.Responses;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.Invoice
{
  public class ReadoutDiscountAmountDistributionCommandTests : FiscalPrinterCommandTestsBase
  {
    private readonly ReadoutDiscountAmountDistributionRequest _sampleRequest = new()
    {
      Amount = 1,
      Action = SimpleSumDiscountAction.Uplift
    };

    [Fact]
    public async Task ReadoutDiscountAmountDistributionCommand_Works()
    {
      var responses = new List<byte> { FiscalPrinterCommands.Esc, FiscalPrinterResponses.ResponseArgument, FiscalPrinterCommands.Mfb, FiscalPrinterCommands.Mfb };
      for (var i = 0; i < Enum.GetNames(typeof(VatClass)).Length - 1; i++)
      {
        responses.AddRange(EncodingHelper.Encode(Faker.Random.Decimal()));
        responses.Add(FiscalPrinterDividers.Lf);
      }

      ResetPrinterWriteRawDataResponse();
      MockAllPrinterResponses(responses.ToArray());
      var request = _sampleRequest;

      var response = await  Run<
        ReadoutDiscountAmountDistributionQuery,
        ReadoutDiscountAmountDistributionRequest,
        ReadoutDiscountAmountDistributionResponse>(request);

      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      DSL.Assert(() => fullCommandString == expectedCommandString);
      DSL.Assert(() => response.Success);
    }

    [Fact]
    public async Task ReadoutDiscountAmountDistributionCommand_Throws_Exception_With_Invalid_Amount()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Amount));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.Amount));
    }

    [Fact]
    public async Task ReadoutDiscountAmountDistributionCommand_Requires_Action()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.Action));
      await AssertArgumentException<ArgumentNullException>(request, nameof(request.Action));
    }

    private List<byte> GetExpectedSentCommand(ReadoutDiscountAmountDistributionRequest request)
    {
      var encodedDiscountAction = request.Action.Encode(nameof(request.Action));
      var encodedAmount = EncodingHelper.Encode(request.Amount.ToString("F"));
      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb,
        
      };
      sentCommand.AddRange(FiscalPrinterDividers.LD);
      sentCommand.AddRange(new[] { encodedDiscountAction });
      sentCommand.AddRange(encodedAmount);
      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });
      return sentCommand;
    }

    private async Task AssertArgumentException<T>(ReadoutDiscountAmountDistributionRequest request, string paramName)
      where T : ArgumentException
    {
      SetupAckRespondingPrinter();

      var exception = await Xunit.Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<
          ReadoutDiscountAmountDistributionQuery,
          ReadoutDiscountAmountDistributionRequest,
          ReadoutDiscountAmountDistributionResponse>(request);
      });
      DSL.Assert(() => exception.Message != null);
      DSL.Assert(() => exception.ParamName == paramName);
    }
  }
}