using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Vera.Poland.Commands.ProtectedMemory;
using Vera.Poland.Models.Responses.ProtectedMemory;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.ProtectedMemory
{
  [SuppressMessage("ReSharper", "StringLiteralTypo")]
  public class OnlineStateReadoutQueryTests : FiscalPrinterCommandTestsBase
  {
    private const string Jpkid = "013367";
    private static readonly byte[] PrinterResponseStartMessage = { FiscalPrinterCommands.Esc, FiscalPrinterResponses.ResponseArgument };
    private static readonly string PrinterResponseWithNoJpkid = $"{EncodingHelper.Decode(PrinterResponseStartMessage)} #FMLT #6/2100,9/25000,0/30";
    private static readonly string PrinterResponseWithJpkid =
      $"{EncodingHelper.Decode(PrinterResponseStartMessage)} NUL 0xD8 ';#IADR#Gliwice,Gliwice,Bojkowska,44-100,35,4; " +
      $"#LJPK#{Jpkid}' SP '2019-06-03T14:06:52;#FMLT #6/2100,9/25000,0/30," +
      "0/200,0/30,3/1000;#LRCP#000001/0007; #LPRN#000004/0007;#TPRN#000062/0007;" +
      "#LSRV#2019-06-03";

    [Fact]
    public async Task When_No_JPKID_Found_Will_Return()
    {
      var convertPrinterResponseToBytes = EncodingHelper.Encode(PrinterResponseWithNoJpkid);
      ResetPrinterWriteRawDataResponse();
      MockExactPrinterResponse(convertPrinterResponseToBytes);

      var response = await  Run<OnlineStateReadoutQuery, OnlineStateReadoutResponse>();

      Assert.False(response.Success);
      Assert.True(response.ResponseMalformed);
      Assert.Null(response.JPKID);

      AssertSentCommand();
    }

    [Fact]
    public async Task Will_Return_JPKID()
    {
      var convertPrinterResponseToBytes = EncodingHelper.Encode(PrinterResponseWithJpkid);
      ResetPrinterWriteRawDataResponse();
      MockExactPrinterResponse(convertPrinterResponseToBytes);

      var response = await  Run<OnlineStateReadoutQuery, OnlineStateReadoutResponse>();

      Assert.Equal(Jpkid, response.JPKID);

      AssertSentCommand();
    }

    private void AssertSentCommand()
    {
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());

      var expectedCommand = new[]
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.L,
        FiscalPrinterDividers.x, FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe
      };

      var expectedCommandString = EncodingHelper.Decode(expectedCommand);
      Assert.Equal(expectedCommandString, fullCommandString);
    }
  }
}