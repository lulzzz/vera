using System.Threading.Tasks;
using Vera.Poland.Commands;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Responses;
using Xunit;

namespace Vera.Poland.Tests.Commands
{
  public class CheckIfPrinterIsInFiscalModeTests : FiscalPrinterCommandTestsBase
  {
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Will_Check_If_Printer_Is_In_Fiscal_Mode(bool printerInFiscalMode)
    {
      SetupExtendedStatusMock(printerInFiscalMode);

      var response = await  Run<CheckIfPrinterIsInFiscalModeQuery, CheckIfPrinterIsInFiscalModeResponse>();

      Assert.Equal(printerInFiscalMode, response.PrinterIsInFiscalMode);
      Assert.True(response.Success);
    }

    private void SetupExtendedStatusMock(bool printerInFiscalMode)
    {
      MockExactPrinterResponse(printerInFiscalMode
        ? ProducePrinterAvailabilityResponse(FiscalStatus.PrinterInFiscalMode, PrinterMechanismStatus.None)
        : ProducePrinterAvailabilityResponse(FiscalStatus.None, PrinterMechanismStatus.None));
    }
  }
}