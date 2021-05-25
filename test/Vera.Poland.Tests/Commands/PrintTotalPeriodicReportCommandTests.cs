using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Poland.Commands;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands
{
  public class PrintTotalPeriodicReportCommandTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task When_Total_Periodic_Report_Type_Is_Not_Set_Will_Throw()
    {
      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<PrintTotalPeriodicReportCommand, PrintTotalPeriodicReportRequest>(new PrintTotalPeriodicReportRequest());
      });

      Assert.Equal(nameof(PrintTotalPeriodicReportRequest.PeriodicReportType), exception.ParamName);
    }

    [Fact]
    public async Task When_No_FromDate_Supplied_For_Report_Type_FromDateToDate_Will_Throw()
    {
      var request = new PrintTotalPeriodicReportRequest
      {
        PeriodicReportType = PeriodicReportType.FromDateToDate,
        ToDate = DateTime.Now
      };

      var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
      {
        await  Run<PrintTotalPeriodicReportCommand, PrintTotalPeriodicReportRequest>(request);
      });

      Assert.Equal(nameof(PrintTotalPeriodicReportRequest.FromDate), exception.ParamName);
    }

    [Fact]
    public async Task When_No_ToDate_Supplied_For_Report_Type_FromDateToDate_Will_Throw()
    {
      var request = new PrintTotalPeriodicReportRequest
      {
        PeriodicReportType = PeriodicReportType.FromDateToDate,
        FromDate = DateTime.Today
      };

      var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
      {
        await  Run<PrintTotalPeriodicReportCommand, PrintTotalPeriodicReportRequest>(request);
      });

      Assert.Equal(nameof(PrintTotalPeriodicReportRequest.ToDate), exception.ParamName);
    }

    [Fact]
    public async Task Will_Send_Correct_Command_To_The_Printer_For_Report_Type_FromDateToDate()
    {
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });
      ResetPrinterWriteRawDataResponse();
      var fromDate = new DateTime(2020, 10, 15);
      var toDate = new DateTime(2021, 1, 3);
      var request = new PrintTotalPeriodicReportRequest
      {
        PeriodicReportType = PeriodicReportType.FromDateToDate,
        FromDate = fromDate,
        ToDate = toDate
      };

      var response = await  Run<PrintTotalPeriodicReportCommand, PrintTotalPeriodicReportRequest>(request);
      Assert.True(response.Success);

      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.H,
        FiscalPrinterDividers.k,
      };
      expectedCommand.AddRange(EncodingHelper.Encode(fromDate.ToString("dd-MM-yy")));
      expectedCommand.Add(FiscalPrinterCommands.Esc);
      expectedCommand.Add(FiscalPrinterCommands.Mfb1);
      expectedCommand.AddRange(EncodingHelper.Encode(toDate.ToString("dd-MM-yy")));
      expectedCommand.Add(FiscalPrinterCommands.Esc);
      expectedCommand.Add(FiscalPrinterCommands.Mfe);

      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());
      Assert.Equal(expectedCommandString, fullCommandString);
    }

    [Fact]
    public async Task Will_Send_Correct_Command_To_The_Printer_For_Report_Type_FromNumberToNumber()
    {
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });
      ResetPrinterWriteRawDataResponse();
      const uint fromNumber = 5;
      const uint toNumber = 10;
      var request = new PrintTotalPeriodicReportRequest
      {
        PeriodicReportType = PeriodicReportType.FromNumberToNumber,
        FromNumber = fromNumber,
        ToNumber = toNumber
      };

      var response = await  Run<PrintTotalPeriodicReportCommand, PrintTotalPeriodicReportRequest>(request);

      Assert.True(response.Success);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.H,
        FiscalPrinterDividers.j,
      };
      expectedCommand.AddRange(EncodingHelper.Encode((int)fromNumber));
      expectedCommand.Add(FiscalPrinterCommands.Esc);
      expectedCommand.Add(FiscalPrinterCommands.Mfb1);
      expectedCommand.AddRange(EncodingHelper.Encode((int)toNumber));
      expectedCommand.Add(FiscalPrinterCommands.Esc);
      expectedCommand.Add(FiscalPrinterCommands.Mfe);

      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());
      Assert.Equal(expectedCommandString, fullCommandString);
    }

    [Fact]
    public async Task When_No_FromNumber_Supplied_For_Report_Type_FromNumberToNumber_Will_Throw()
    {
      var request = new PrintTotalPeriodicReportRequest
      {
        PeriodicReportType = PeriodicReportType.FromNumberToNumber
      };

      var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
      {
        await  Run<PrintTotalPeriodicReportCommand, PrintTotalPeriodicReportRequest>(request);
      });

      Assert.Equal(nameof(PrintTotalPeriodicReportRequest.FromNumber), exception.ParamName);
    }

    [Fact]
    public async Task When_No_ToNumber_Supplied_For_Report_Type_FromNumberToNumber_Will_Throw()
    {
      var request = new PrintTotalPeriodicReportRequest
      {
        PeriodicReportType = PeriodicReportType.FromNumberToNumber,
        FromNumber = 1
      };

      var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
      {
        await  Run<PrintTotalPeriodicReportCommand, PrintTotalPeriodicReportRequest>(request);
      });

      Assert.Equal(nameof(PrintTotalPeriodicReportRequest.ToNumber), exception.ParamName);
    }

    [Fact]
    public async Task When_Total_Monthly_Report_Is_Null_Will_Throw()
    {
      var request = new PrintTotalPeriodicReportRequest
      {
        PeriodicReportType = PeriodicReportType.TotalMonthlyFiscalReport
      };

      var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
      {
        await  Run<PrintTotalPeriodicReportCommand, PrintTotalPeriodicReportRequest>(request);
      });

      Assert.Equal(nameof(PrintTotalPeriodicReportRequest.TotalMonthlyReport), exception.ParamName);
    }

    [Fact]
    public async Task When_Current_Month_Requested_For_Report_Type_TotalMonthlyFiscalReport_Will_Throw()
    {
      var request = new PrintTotalPeriodicReportRequest
      {
        PeriodicReportType = PeriodicReportType.TotalMonthlyFiscalReport,
        TotalMonthlyReport = DateTime.Today
      };

      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<PrintTotalPeriodicReportCommand, PrintTotalPeriodicReportRequest>(request);
      });

      Assert.Equal(nameof(PrintTotalPeriodicReportRequest.TotalMonthlyReport), exception.ParamName);
    }

    [Fact]
    public async Task Will_Send_Correct_Command_To_The_Printer_For_Report_Type_TotalMonthlyFiscalReport()
    {
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });
      ResetPrinterWriteRawDataResponse();
      var monthlyReport = new DateTime(2020, 10, 15);
      var request = new PrintTotalPeriodicReportRequest
      {
        PeriodicReportType = PeriodicReportType.TotalMonthlyFiscalReport,
        TotalMonthlyReport = monthlyReport
      };

      var response = await  Run<PrintTotalPeriodicReportCommand, PrintTotalPeriodicReportRequest>(request);

      Assert.True(response.Success);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.H,
        FiscalPrinterDividers.m,
        FiscalPrinterDividers.s,
      };
      expectedCommand.AddRange(EncodingHelper.Encode(monthlyReport.ToString("MM-yy")));
      expectedCommand.Add(FiscalPrinterCommands.Esc);
      expectedCommand.Add(FiscalPrinterCommands.Mfe);

      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());
      Assert.Equal(expectedCommandString, fullCommandString);
    }

    [Fact]
    public async Task Will_Send_Correct_Command_To_The_Printer_For_Report_Type_EntireMemory()
    {
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });
      ResetPrinterWriteRawDataResponse();
      var request = new PrintTotalPeriodicReportRequest
      {
        PeriodicReportType = PeriodicReportType.EntireMemory,
      };

      var response = await  Run<PrintTotalPeriodicReportCommand, PrintTotalPeriodicReportRequest>(request);

      Assert.True(response.Success);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.H,
        FiscalPrinterDividers.b,
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfe
      };

      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());
      Assert.Equal(expectedCommandString, fullCommandString);
    }
  }
}