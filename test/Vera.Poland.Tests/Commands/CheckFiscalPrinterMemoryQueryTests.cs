using System;
using System.Threading.Tasks;
using Vera.Poland.Commands;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Responses;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands
{
  public class CheckFiscalPrinterMemoryQueryTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task If_Printer_Response_Is_Not_In_Expected_Format_Will_Throw()
    {
      MockSinglePrinterResponse(new[] { FiscalPrinterResponses.Nak });

      await Assert.ThrowsAsync<InvalidOperationException>(async () =>
      {
        await  Run<CheckFiscalPrinterMemoryQuery, FiscalPrinterMemoryResponse>();
      });
    }

    [Fact]
    public async Task If_Printer_Response_Is_Not_Success_Will_Return()
    {
      MockSinglePrinterResponse(new byte[12]);

      var response = await  Run<CheckFiscalPrinterMemoryQuery, FiscalPrinterMemoryResponse>();

      Assert.False(response.Success);
      Assert.True(response.ResponseMalformed);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public async Task Will_Check_Printer_Memory(bool expectedFiscalMemoryFull, bool expectedFiscalMemoryAlmostFull)
    {
      SetupExtendedStatusMock(expectedFiscalMemoryFull, expectedFiscalMemoryAlmostFull);
      var response = await  Run<CheckFiscalPrinterMemoryQuery, FiscalPrinterMemoryResponse>();

      AssertSuccessfulMessage(response, expectedFiscalMemoryFull, expectedFiscalMemoryAlmostFull);
    }

    private void SetupExtendedStatusMock(bool isPrinterMemoryFull, bool isPrinterMemoryAlmostFull)
    {
      var fiscalStatus = FiscalStatus.None;

      if (isPrinterMemoryAlmostFull)
      {
        fiscalStatus |= FiscalStatus.FiscalMemoryAlmostFull;
      }

      if (isPrinterMemoryFull)
      {
        fiscalStatus |= FiscalStatus.FiscalMemoryFull;
      }

      const PrinterMechanismStatus printerMechanismStatus = PrinterMechanismStatus.PrinterMechanismIsReady;

      var extendedStatusResponse = ProducePrinterAvailabilityResponse(fiscalStatus, printerMechanismStatus);

      MockSinglePrinterResponse(extendedStatusResponse);
    }

    private static void AssertSuccessfulMessage(
      FiscalPrinterMemoryResponse response,
      bool expectedFiscalMemoryFull,
      bool expectedFiscalMemoryAlmostFull)
    {
      Assert.True(response.Success);
      Assert.Equal(expectedFiscalMemoryFull, response.FiscalMemoryFull);
      Assert.Equal(expectedFiscalMemoryAlmostFull, response.FiscalMemoryAlmostFull);
    }
  }
}