using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assertive;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Models.Responses;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.Invoice
{
  public class RequestAutoTotalCommandTests : FiscalPrinterCommandTestsBase
  {
    private const string ReadoutParameter = "tra.tot";

    [Fact]
    public async Task RequestAutoTotalCommand_Works_With_Sample()
    {
      const string inputSample = "+000000000000021,69";
      await AssertSuccessfulRequestAutoTotal(inputSample);
    }

    [Fact]
    public async Task RequestAutoTotalCommand_Works_With_Random_Decimal()
    {
      var inputSample = $"{Faker.Random.Int(0,9999999)},{Faker.Random.Int(0,99)}";
      await AssertSuccessfulRequestAutoTotal(inputSample);
    }

    [Fact]
    public async Task RequestAutoTotalCommand_Requires_Specific_Sequence()
    {
      await AssertException<IndexOutOfRangeException>();
    }

    private async Task AssertSuccessfulRequestAutoTotal(string inputSample)
    {
      var responses = new List<byte> { FiscalPrinterCommands.Esc, FiscalPrinterResponses.ResponseArgument, FiscalPrinterCommands.Esc, FiscalPrinterCommands.Esc, FiscalPrinterCommands.Esc };
      responses.AddRange(EncodingHelper.Encode(inputSample));

      ResetPrinterWriteRawDataResponse();
      MockAllPrinterResponses(responses.ToArray());

      var response = await  Run<RequestAutoTotalQuery, AutoTotalResponse>();
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand();
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      DSL.Assert(() => fullCommandString == expectedCommandString);
      DSL.Assert(() => response.Success);
      DSL.Assert(() => inputSample == response.Raw);
    }
    private static List<byte> GetExpectedSentCommand()
    {
      var readoutParameterEncoded = EncodingHelper.Encode(ReadoutParameter);
      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb,
      };
      sentCommand.AddRange(FiscalPrinterDividers.LT);
      sentCommand.AddRange(readoutParameterEncoded);
      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });
      return sentCommand;
    }

    private async Task AssertException<T>()
      where T : Exception
    {
      SetupAckRespondingPrinter();

      var exception = await Xunit.Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<RequestAutoTotalQuery, AutoTotalResponse>();
      });
      DSL.Assert(() => exception.Message != null);
    }
  }
}