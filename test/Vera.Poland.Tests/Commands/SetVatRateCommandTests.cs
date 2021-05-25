using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assertive;
using Vera.Poland.Commands;
using Vera.Poland.Helpers;
using Vera.Poland.Models;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;
using Xunit;
using Assert = Xunit.Assert;
using static Assertive.DSL;

namespace Vera.Poland.Tests.Commands
{
  public class SetVatRateCommandTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task Printer_Set_Vat_Rate_Error()
    {
      ResetPrinterWriteRawDataResponse();
      var request = new SetVatRequest
      {
        A = new VatItem
        {
          RatePercentage = 20
        },
        B = new VatItem
        {
          RatePercentage = 9
        },
        C = new VatItem
        {
          RatePercentage = 5
        },
        D = new VatItem
        {
          RatePercentage = 0
        },
        E = new VatItem { IsNotSet = true },
        F = new VatItem { IsNotSet = true },
        G = new VatItem { IsExemptFromTax = true }
      };

      var printerAckResponse = new[] { FiscalPrinterResponses.Nak };
      MockSinglePrinterResponse(printerAckResponse);

      var response = await  Run<SetVatRateCommand, SetVatRequest>(request);
      Assert(() => !response.Success);
    }

    [Fact]
    public async Task Printer_Set_Vat_Rate_Non_Exempt_Non_Null_Success()
    {
      var request = ProvideNonExemptAndNonNullVatRequest();
      await Printer_Set_Vat_Rate_Success(request);
    }

    [Fact]
    public async Task Printer_Set_Vat_Rate_Non_Exempt_Null_Success()
    {
      var request = ProvideNonExemptAndNullVatRequest();
      await Printer_Set_Vat_Rate_Success(request);
    }

    [Fact]
    public async Task Printer_Set_Vat_Rate_Exempt_Non_Null_Success()
    {
      var request = ProvideExemptAndNonNullVatRequest();
      await Printer_Set_Vat_Rate_Success(request);
    }

    [Fact]
    public async Task Printer_Set_Vat_Rate_Exempt_Null_Success()
    {
      var request = ProvideExemptAndNullVatRequest();
      await Printer_Set_Vat_Rate_Success(request);
    }

    private async Task Printer_Set_Vat_Rate_Success(SetVatRequest request)
    {
      ResetPrinterWriteRawDataResponse();
      var printerAckResponse = new[] { FiscalPrinterResponses.Ack };
      MockSinglePrinterResponse(printerAckResponse);

      var response = await  Run<SetVatRateCommand, SetVatRequest>(request);

      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      DSL.Assert(() => fullCommandString == expectedCommandString);
      DSL.Assert(() => response.Success);
    }

    [Fact]
    public void Vat_Item_Not_Set()
    {
      var formatting = VatRateHelper.GetVatValue(new VatItem
      {
        IsNotSet = true
      });

      Assert.Equal(VatRateHelper.NullFormatting, formatting);
    }

    [Fact]
    public void Exempt_From_Tax_Vat_Item()
    {
      var formatting = VatRateHelper.GetVatValue(new VatItem { IsExemptFromTax = true });

      Assert.Equal(VatRateHelper.ExemptFromTaxFormatting, formatting);
    }

    [Fact]
    public void Out_Of_Range_Vat_Item()
    {
      var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
      {
        VatRateHelper.GetVatValue(new VatItem { RatePercentage = 111 });
      });

      Assert.Equal(nameof(VatItem), error.ParamName);
    }

    [Fact]
    public void Correct_Vat_Item()
    {
      var formatting = VatRateHelper.GetVatValue(new VatItem { RatePercentage = 15 });

      Assert.Equal("1500", formatting);
    }

    private static SetVatRequest ProvideNonExemptAndNonNullVatRequest()
    {
      var request = new SetVatRequest
      {
        A = new VatItem { RatePercentage = 15 },
        B = new VatItem { RatePercentage = 25 },
        C = new VatItem { RatePercentage = 35 },
        D = new VatItem { RatePercentage = 45 },
        E = new VatItem { RatePercentage = 55 },
        F = new VatItem { RatePercentage = 65 },
        G = new VatItem { RatePercentage = 75 }
      };

      return request;
    }

    private static SetVatRequest ProvideNonExemptAndNullVatRequest()
    {
      var request = new SetVatRequest
      {
        A = new VatItem { RatePercentage = 15 },
        B = new VatItem { RatePercentage = 25 },
        C = new VatItem { RatePercentage = 35 },
        D = new VatItem { RatePercentage = 45 },
        E = new VatItem { RatePercentage = 55 },
        F = new VatItem { RatePercentage = 65 },
        G = new VatItem { RatePercentage = 75 }
      };

      return request;
    }

    private static SetVatRequest ProvideExemptAndNonNullVatRequest()
    {
      var request = new SetVatRequest
      {
        A = new VatItem { RatePercentage = 15 },
        B = new VatItem { RatePercentage = 25 },
        C = new VatItem { RatePercentage = 35 },
        D = new VatItem { RatePercentage = 45 },
        E = new VatItem { IsExemptFromTax = true },
        F = new VatItem { RatePercentage = 65 },
        G = new VatItem { IsExemptFromTax = true}
      };

      return request;
    }

    private static SetVatRequest ProvideExemptAndNullVatRequest()
    {
      var request = new SetVatRequest
      {
        A = new VatItem { RatePercentage = 15 },
        B = new VatItem { RatePercentage = 25 },
        C = new VatItem { RatePercentage = 35 },
        D = new VatItem { RatePercentage = 45 },
        E = new VatItem { IsExemptFromTax = true },
        F = new VatItem
        {
          IsNotSet = true
        },
        G = new VatItem { IsExemptFromTax = true }
      };

      return request;
    }

    private static IEnumerable<byte> GetExpectedSentCommand(SetVatRequest request)
    {
      byte[] GetVatEncoded()
      {
        var vatRateBuilder = new StringBuilder();

        var rates = new[]
        {
          request.A,
          request.B,
          request.C,
          request.D,
          request.E,
          request.F,
          request.G
        };

        foreach (var vatRate in rates)
        {
          vatRateBuilder.Append(VatRateHelper.GetVatValue(vatRate));
        }

        var vatData = vatRateBuilder.ToString();
        return EncodingHelper.Encode(vatData);
      }

      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb
      };

      sentCommand.AddRange(FiscalPrinterDividers.Kd);
      sentCommand.AddRange(GetVatEncoded());
      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });

      return sentCommand;
    }
  }
}