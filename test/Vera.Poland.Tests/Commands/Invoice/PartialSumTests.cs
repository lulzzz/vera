using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assertive;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.Invoice
{
  public class PartialSumTests : FiscalPrinterCommandTestsBase
  {
    private readonly PartialSumRequest _sampleRequest = new()
    {
      PartialSum = 1
    };

    [Fact]
    public async Task PartialSumCommand_Works()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;

      var response = await  Run<PartialSumCommand, PartialSumRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      DSL.Assert(() => fullCommandString == expectedCommandString);
      DSL.Assert(() => response.Success);
    }

    [Fact]
    public async Task PartialSumCommand_Requires_PartialSum()
    {
      var request = CloneExcludingProperty(_sampleRequest, nameof(_sampleRequest.PartialSum));
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.PartialSum));
    }

    [Fact]
    public async Task PartialSumCommand_Requires_PartialSum_Greater_Than_0()
    {
      var request = _sampleRequest;
      request.PartialSum = -5;
      await AssertArgumentException<ArgumentOutOfRangeException>(request, nameof(request.PartialSum));
    }

    private List<byte> GetExpectedSentCommand(PartialSumRequest request)
    {
      var encodedPartialSum = EncodingHelper.Encode(request.PartialSum);

      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.Q,
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb1
      };
      sentCommand.AddRange(encodedPartialSum);
      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });

      return sentCommand;
    }

    private async Task AssertArgumentException<T>(PartialSumRequest request, string paramName)
      where T : ArgumentException
    {
      SetupAckRespondingPrinter();
      var exception = await Xunit.Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<PartialSumCommand, PartialSumRequest>(request);
      });
      DSL.Assert(() => exception.Message != null);
      DSL.Assert(() => exception.ParamName == paramName);
    }
  }
}