using System;
using System.Threading.Tasks;
using Vera.Poland.Commands;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands
{
  public class PrintFiscalDailyReportCommandTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task When_Printer_Does_Not_Return_Response_Will_Throw()
    {
      MockExactPrinterResponse(new byte[0]);

      var request = new PrintFiscalDailyReportRequest
      {
        DigitalReportOnly = true
      };

      await Assert.ThrowsAsync<InvalidOperationException>(async () =>
      {
        await  Run<PrintFiscalDailyReportCommand, PrintFiscalDailyReportRequest>(request);
      });

      AssertCommandSentToPrinter();
    }

    [Fact]
    public async Task Will_Handle_Printed_Report()
    {
      var request = new PrintFiscalDailyReportRequest
      {
        DigitalReportOnly = false
      };

      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });

      var result = await  Run<PrintFiscalDailyReportCommand, PrintFiscalDailyReportRequest>(request);
      Assert.True(result.Success);

      AssertCommandSentToPrinter(false);
    }

    [Fact]
    public async Task Will_Handle_Digital_Report()
    {
      var request = new PrintFiscalDailyReportRequest
      {
        DigitalReportOnly = true
      };

      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack, byte.MaxValue, byte.MinValue
    });

      var result = await  Run<PrintFiscalDailyReportCommand, PrintFiscalDailyReportRequest>(request);
      Assert.True(result.Success);

      AssertCommandSentToPrinter();
    }

    private void AssertCommandSentToPrinter(bool digitalOnly = true)
    {
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = digitalOnly ? new[]
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.B,
        FiscalPrinterDividers.Lf,
        FiscalPrinterDividers.c,
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfe
      } :
      new[]
      {
          FiscalPrinterCommands.Esc,
          FiscalPrinterCommands.Mfb,
          FiscalPrinterDividers.B,
          FiscalPrinterCommands.Esc,
          FiscalPrinterCommands.Mfe
      };

      var expectedCommandString = EncodingHelper.Decode(expectedCommand);
      Assert.Equal(expectedCommandString, fullCommandString);
    }
  }
}