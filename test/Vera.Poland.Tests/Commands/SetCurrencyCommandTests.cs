using System;
using System.Linq;
using System.Threading.Tasks;
using Vera.Poland.Commands;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;
using Xunit;
using static Assertive.DSL;

#pragma warning disable 1998

namespace Vera.Poland.Tests.Commands
{
  public class SetCurrencyCommandTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task Printer_Set_Currency_With_Date_Without_Time_Success()
    {
      var date = new DateTime(2021, 02, 01);
      const string formattedDatePart = "01-02-21";

      ResetPrinterWriteRawDataResponse();

      var printerAckResponse = new[] { FiscalPrinterResponses.Ack };
      MockSinglePrinterResponse(printerAckResponse);

      var request = new SetCurrencyRequest
      {
        Date = date,
        CurrencyCode = "EUR"
      };

      var response = await  Run<SetCurrencyCommand, SetCurrencyRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());

      var expectedCommand = new[]
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        Convert.ToByte('K'),
        Convert.ToByte('E'),
      };

      var encodedDatePart = EncodingHelper.Encode(formattedDatePart);
      var encodedCurrencyCode = EncodingHelper.Encode(request.CurrencyCode);

      var finalCommand = expectedCommand.Concat(encodedDatePart).ToList();
      finalCommand.Add(FiscalPrinterDividers.Lf);
      finalCommand = finalCommand.Concat(encodedCurrencyCode).ToList();
      finalCommand.Add(FiscalPrinterDividers.Lf);
      finalCommand.Add(FiscalPrinterCommands.Esc);
      finalCommand.Add(FiscalPrinterCommands.Mfe);

      var expectedCommandString = EncodingHelper.Decode(finalCommand.ToArray());

      Assert(() => fullCommandString == expectedCommandString);
      Assert(() => response.Success);
    }

    [Fact]
    public async Task Printer_Set_Currency_With_Date_And_Time_Success()
    {
      var date = new DateTime(2021, 02,01, 09, 09, 30);
      const string formattedDatePart = "01-02-21";
      const string formattedTimePart = "09-09";

      ResetPrinterWriteRawDataResponse();

      var printerAckResponse = new[] { FiscalPrinterResponses.Ack };
      MockSinglePrinterResponse(printerAckResponse);

      var request = new SetCurrencyRequest
      {
        Date = date,

        CurrencyCode = "EUR"
      };

      var response = await  Run<SetCurrencyCommand, SetCurrencyRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());

      var expectedCommand = new[]
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        Convert.ToByte('K'),
        Convert.ToByte('E'),
      };

      var encodedDatePart = EncodingHelper.Encode(formattedDatePart);
      var encodedTimePart = EncodingHelper.Encode(formattedTimePart);
      var encodedCurrencyCode = EncodingHelper.Encode(request.CurrencyCode);

      var finalCommand = expectedCommand.Concat(encodedDatePart).ToList();
      finalCommand.Add(FiscalPrinterDividers.Sp);
      finalCommand = finalCommand.Concat(encodedTimePart).ToList();
      finalCommand.Add(FiscalPrinterDividers.Lf);
      finalCommand = finalCommand.Concat(encodedCurrencyCode).ToList();
      finalCommand.Add(FiscalPrinterDividers.Lf);
      finalCommand.Add(FiscalPrinterCommands.Esc);
      finalCommand.Add(FiscalPrinterCommands.Mfe);

      var expectedCommandString = EncodingHelper.Decode(finalCommand.ToArray());

      Assert(() => fullCommandString == expectedCommandString);
      Assert(() => response.Success);
    }

    [Fact]
    public async Task Printer_Set_Currency_Success()
    {
      ResetPrinterWriteRawDataResponse();

      var printerAckResponse = new[] { FiscalPrinterResponses.Ack };
      MockSinglePrinterResponse(printerAckResponse);

      var request = new SetCurrencyRequest
      {
        Date = null,
        CurrencyCode = "EUR"
      };

      var response = await  Run<SetCurrencyCommand, SetCurrencyRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());

      var expectedCommand = new[]
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        Convert.ToByte('K'),
        Convert.ToByte('E'),
        FiscalPrinterDividers.Lf,
      };

      var encodedCurrencyCode = EncodingHelper.Encode(request.CurrencyCode);

      var commandWithCurrency = expectedCommand.Concat(encodedCurrencyCode).ToList();
      commandWithCurrency.Add(FiscalPrinterDividers.Lf);
      commandWithCurrency.Add(FiscalPrinterCommands.Esc);
      commandWithCurrency.Add(FiscalPrinterCommands.Mfe);

      var expectedCommandString = EncodingHelper.Decode(commandWithCurrency.ToArray());

      Assert(() => fullCommandString == expectedCommandString);
      Assert(() => response.Success);
    }
  }
}