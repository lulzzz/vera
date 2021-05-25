using System;
using System.Linq;
using System.Threading.Tasks;
using Vera.Poland.Commands;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Responses;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands
{
  public class ReadExtendedStatusCommandTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task Printer_Status_Is_Correctly_Computed()
    {
      ResetPrinterWriteRawDataResponse();

      const FiscalStatus expectedFiscalStatus = FiscalStatus.HardwareDataSavedInFiscalMemory
                                                | FiscalStatus.ProducerDataSavedInFiscalMemory
                                                | FiscalStatus.UniqueNumberSavedInFiscalMemory
                                                | FiscalStatus.PrinterInFiscalMode
                                                | FiscalStatus.VatRatesArraySavedInFiscalMemory
                                                | FiscalStatus.FiscalMode
                                                | FiscalStatus.HeaderDefined
                                                | FiscalStatus.FiscalMemoryConnected
                                                | FiscalStatus.DisplayConnected
                                                | FiscalStatus.AlphanumericalDisplay;

      MockExactPrinterResponse(new byte[]
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterResponses.ResponseArgument,
        FiscalPrinterDividers.Nul,
        0x08,
        FiscalPrinterDividers.Nul,
        0x1F,
        0x21,
        0x1A,
        0x10,
        FiscalPrinterDividers.Nul,
        FiscalPrinterDividers.Nul,
        FiscalPrinterDividers.Nul
      });

      var result = await  Run<ReadExtendedStatusQuery, ExtendedStatusResponse>();


      Assert.True(result.Success);
      Assert.Equal(result.FiscalStatus, expectedFiscalStatus);
    }

    [Fact]
    public async Task When_The_Printer_Does_Not_Return_Response_Will_Throw()
    {
      MockExactPrinterResponse(new byte[0]);

      await Assert.ThrowsAsync<InvalidOperationException>(async () =>
      {
        await  Run<ReadExtendedStatusQuery, ExtendedStatusResponse>();
      });

      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());

      var expectedCommand = new[]
      {
        FiscalPrinterCommands.Esc
      }.Concat(FiscalPrinterArguments.ReadExtendedStatusArgument).ToArray();

      var expectedCommandString = EncodingHelper.Decode(expectedCommand);
      Assert.Equal(expectedCommandString, fullCommandString);
    }
  }
}
