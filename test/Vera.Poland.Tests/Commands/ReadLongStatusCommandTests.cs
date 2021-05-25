using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vera.Poland.Commands;
using Vera.Poland.Models.Responses;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands
{
  public class ReadLongStatusCommandTests : FiscalPrinterCommandTestsBase
  {

    [Fact]
    public async Task When_The_Printer_Does_Not_Return_Response_Will_Throw()
    {
      MockExactPrinterResponse(new byte[0]);

      await Assert.ThrowsAsync<InvalidOperationException>(async () =>
      {
        await  Run<ReadLongStatusQuery, LongStatusResponse>();
      });

      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());

      var expectedCommand = new[]
      {
        FiscalPrinterCommands.Esc
      }.Concat(FiscalPrinterArguments.LongStatusArgument).ToArray();

      var expectedCommandString = EncodingHelper.Decode(expectedCommand);
      Assert.Equal(expectedCommandString, fullCommandString);
    }

    [Fact]
    public async Task Response_Is_Malformed()
    {
      ResetPrinterWriteRawDataResponse();

      MockExactPrinterResponse(_humanizedMalformedPrinterResponse);

      var result = await  Run<ReadLongStatusQuery, LongStatusResponse>();

      Assert.False(result.Success);
      Assert.True(result.ResponseMalformed);
    }

    [Fact]
    public async Task Status_Is_Correctly_Computed()
    {
      ResetPrinterWriteRawDataResponse();

      MockExactPrinterResponse(_humanizedPrinterResponse);

      var result = await  Run<ReadLongStatusQuery, LongStatusResponse>();

      Assert.True(result.Success);
      Assert.True(result.IsDisplayTypeAlphanumeric);
      Assert.True(result.IsFiscalDayOpen);
      Assert.False(result.IsFiscalDeviceInReadonlyMode);
      Assert.False(result.IsNonFiscalDocumentPrintoutOngoing);
      Assert.False(result.IsPreFiscal);
      Assert.False(result.IsReceiptOpen);
      Assert.False(result.IsReceiptSummarized);
      Assert.False(result.IsXonXoffProtocolOn);
      Assert.False(result.ResponseMalformed);
      Assert.Equal(result.LastDailyReportDate, new DateTime(2021, 5, 21));
      Assert.Equal("0232", result.LastDailyReportNumber);
      Assert.Equal(result.PrinterTime, new DateTime(2021, 05, 24, 11, 15, 25));
      Assert.Equal("00005", result.ReceiptCounter);
    }

    private readonly byte[] _humanizedPrinterResponse = new[]
    {
      FiscalPrinterCommands.Esc,
      FiscalPrinterResponses.ResponseArgument,
      (byte) 0x00, // MSB - ignored
      (byte) 0x4A, // LSB - ignored
      (byte) 0x04, // status byte
      (byte) 0x37, // configuration byte
      (byte) 0x32, (byte) 0x35, (byte) 0x31, (byte) 0x35, (byte) 0x31, (byte) 0x31, (byte) 0x32, (byte) 0x34, (byte) 0x30, (byte) 0x35, (byte) 0x32, (byte) 0x31, // date and time
      (byte) 0x30, // separator
      (byte) 0x30, (byte) 0x30, (byte) 0x30, (byte) 0x30, (byte) 0x35, // receipt counter
      (byte) 0x32, (byte) 0x31, (byte) 0x30, (byte) 0x35, (byte) 0x32, (byte) 0x31, // last daily record date
      (byte) 0x30, (byte) 0x32, (byte) 0x33, (byte) 0x32, // last daily report number
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul // 44 occurrences of 0x20 at the end
    };

    private readonly byte[] _humanizedMalformedPrinterResponse = new[]
    {
      FiscalPrinterCommands.Mfb, // not passing esc on purpose
      FiscalPrinterResponses.Lda, //  not passing r on purpose
      (byte) 0x00, // MSB - ignored
      (byte) 0x4A, // LSB - ignored
      (byte) 0x04, // status byte
      (byte) 0x37, // configuration byte
      (byte) 0x32, (byte) 0x35, (byte) 0x31, (byte) 0x35, (byte) 0x31, (byte) 0x31, (byte) 0x32, (byte) 0x34, (byte) 0x30, (byte) 0x35, (byte) 0x32, (byte) 0x31, // date and time
      (byte) 0x30, // separator
      (byte) 0x30, (byte) 0x30, (byte) 0x30, (byte) 0x30, (byte) 0x35, // receipt counter
      (byte) 0x32, (byte) 0x31, (byte) 0x30, (byte) 0x35, (byte) 0x32, (byte) 0x31, // last daily record date
      (byte) 0x30, (byte) 0x32, (byte) 0x33, (byte) 0x32, // last daily report number
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul,
      FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul, FiscalPrinterDividers.Nul // 44 occurrences of 0x20 at the end
    };

    /// <summary>
    /// Helper method - do not delete
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    private static List<string> ByteArrayToHexArray(IEnumerable<byte> bytes) {
      var lookupTable = Enumerable.Range(0, 255).Select(i => {
        var s = i.ToString("X2");
        return ((uint)s[0]) + ((uint)s[1] << 16);
      }).ToArray();

      return bytes.Select(t => lookupTable[t]).Select(val => $"0x{(char) val}{(char) (val >> 16)}").ToList();
    }
  }
}