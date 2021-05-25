using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vera.Extensions;
using Vera.Poland.Commands;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;
using Xunit;
using Assert = Xunit.Assert;
using static Assertive.DSL;

namespace Vera.Poland.Tests.Commands
{
  public class SetTaxpayerCommandTests : FiscalPrinterCommandTestsBase
  {

    [Fact]
    public async Task Printer_Set_Taxpayer_Error()
    {
      ResetPrinterWriteRawDataResponse();
      var request = new SetTaxpayerNameRequest
      {
        TaxpayerLines = new List<string>
        {
          "a", "b", "c", "d", "e"
        }
      };
      var printerAckResponse = new[] { FiscalPrinterResponses.Nak };
      MockSinglePrinterResponse(printerAckResponse);

      var response = await  Run<SetTaxpayerCommand, SetTaxpayerNameRequest>(request);

      Assert(() => !response.Success);
    }

    [Fact]
    public async Task Printer_Set_Taxpayer_Empty()
    {
      ResetPrinterWriteRawDataResponse();
      var request = new SetTaxpayerNameRequest();

      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<SetTaxpayerCommand, SetTaxpayerNameRequest>(request);
      });

      Assert.Equal(nameof(request.TaxpayerLines), exception.ParamName);
      Assert.Equal("Taxpayer lines empty (Parameter 'TaxpayerLines')", exception.Message);
    }

    [Fact]
    public async Task Printer_Set_Taxpayer_Too_Many_Lines()
    {
      ResetPrinterWriteRawDataResponse();
      var request = new SetTaxpayerNameRequest
      {
        TaxpayerLines = new List<string>
        {
          "a", "b", "c", "d", "e", "f", "g", "h"
        }
      };

      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<SetTaxpayerCommand, SetTaxpayerNameRequest>(request);
      });

      Assert.Equal(nameof(request.TaxpayerLines), exception.ParamName);
      Assert.Equal("Taxpayer lines must be at most 6 (Parameter 'TaxpayerLines')", exception.Message);
    }

    [Fact]
    public async Task Printer_Set_Taxpayer_Success_With_Non_Empty_Lines()
    {
      var request = new SetTaxpayerNameRequest
      {
        TaxpayerLines = new List<string>
        {
          "a", "b", "c", "d", "e", "f"
        }
      };

      await Printer_Set_Taxpayer_Success(request);
    }

    [Fact]
    public async Task Printer_Set_Taxpayer_Success_With_Some_Empty_Lines()
    {
      var request = new SetTaxpayerNameRequest
      {
        TaxpayerLines = new List<string>
        {
          "a", "b", "", "d", "", "f"
        }
      };

      await Printer_Set_Taxpayer_Success(request);
    }

    private async Task Printer_Set_Taxpayer_Success(SetTaxpayerNameRequest request)
    {
      ResetPrinterWriteRawDataResponse();
      var printerAckResponse = new[] { FiscalPrinterResponses.Ack };
      MockSinglePrinterResponse(printerAckResponse);

      var response = await  Run<SetTaxpayerCommand, SetTaxpayerNameRequest>(request);

      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      Assert(() => fullCommandString == expectedCommandString);
      Assert(() => response.Success);
    }

    private static IEnumerable<byte> GetExpectedSentCommand(SetTaxpayerNameRequest request)
    {
      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.h
      };

      var encodedTaxPayerLines = request.TaxpayerLines
        .Where(line => !line.IsNullOrWhiteSpace())
        .Select(EncodingHelper.Encode).ToList();

      foreach (var encodedLine in encodedTaxPayerLines)
      {
        sentCommand.AddRange(encodedLine);

        // we divide each line by linefeed
        //
        sentCommand.Add(FiscalPrinterDividers.Lf);
      }

      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });

      return sentCommand;
    }
  }
}