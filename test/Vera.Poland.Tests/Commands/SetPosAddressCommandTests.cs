using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vera.Poland.Commands;
using Vera.Poland.Helpers;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;
using Xunit;
using Assert = Xunit.Assert;
using static Assertive.DSL;

namespace Vera.Poland.Tests.Commands
{
  public class SetPosAddressCommandTests : FiscalPrinterCommandTestsBase
  {

    [Fact]
    public async Task Printer_Set_Pos_Address_Invalid_Place()
    {
      ResetPrinterWriteRawDataResponse();
      var request = new SetPosAddressRequest
      {
        Place = "Very long place description with more than 30 characters"
      };

      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<SetPosAddressCommand, SetPosAddressRequest>(request);
      });

      Assert.Equal(nameof(request.Place), exception.ParamName);
    }

    [Fact]
    public async Task Printer_Set_Pos_Address_Invalid_TaxOffice()
    {
      ResetPrinterWriteRawDataResponse();
      var request = new SetPosAddressRequest
      {
        Place = "Valid Place",
        TaxOffice = "Very long tax office description with more than 30 characters"
      };
      
      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<SetPosAddressCommand, SetPosAddressRequest>(request);
      });

      Assert.Equal(nameof(request.TaxOffice), exception.ParamName);
    }

    [Fact]
    public async Task Printer_Set_Pos_Address_Invalid_Street()
    {
      ResetPrinterWriteRawDataResponse();
      var request = new SetPosAddressRequest
      {
        Place = "Valid Place",
        TaxOffice = "Valid TaxOffice",
        Street = "Very long street description with more than 30 characters"
      };

      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<SetPosAddressCommand, SetPosAddressRequest>(request);
      });

      Assert.Equal(nameof(request.Street), exception.ParamName);
    }

    [Fact]
    public async Task Printer_Set_Pos_Address_Invalid_PostalCode()
    {
      ResetPrinterWriteRawDataResponse();
      var request = new SetPosAddressRequest
      {
        Place = "Valid Place",
        TaxOffice = "Valid TaxOffice",
        Street = "Valid Street",
        PostalCode = "Very long postal code description with more than 20 characters"
      };

      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<SetPosAddressCommand, SetPosAddressRequest>(request);
      });

      Assert.Equal(nameof(request.PostalCode), exception.ParamName);
    }

    [Fact]
    public async Task Printer_Set_Pos_Address_Invalid_HouseNumber()
    {
      ResetPrinterWriteRawDataResponse();
      var request = new SetPosAddressRequest
      {
        Place = "Valid Place",
        TaxOffice = "Valid TaxOffice",
        Street = "Valid Street",
        PostalCode = "112254",
        HouseNumber = "Very long house number description with more than 20 characters"
      };

      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<SetPosAddressCommand, SetPosAddressRequest>(request);
      });

      Assert.Equal(nameof(request.HouseNumber), exception.ParamName);
    }

    [Fact]
    public async Task Printer_Set_Pos_Address_Invalid_Apartment_Number()
    {
      ResetPrinterWriteRawDataResponse();
      var request = new SetPosAddressRequest
      {
        Place = "Valid Place",
        TaxOffice = "Valid TaxOffice",
        Street = "Valid Street",
        PostalCode = "112254",
        HouseNumber = "115",
        ApartmentNumber = "Very long apartment number description with more than 20 characters"
      };

      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<SetPosAddressCommand, SetPosAddressRequest>(request);
      });

      Assert.Equal(nameof(request.ApartmentNumber), exception.ParamName);
    }

    [Fact]
    public async Task Printer_Set_Pos_Address_Success()
    {
      ResetPrinterWriteRawDataResponse();
      var request = new SetPosAddressRequest
      {
        Place = "Valid Place",
        TaxOffice = "Valid TaxOffice",
        Street = "Valid Street",
        PostalCode = "112254",
        HouseNumber = "115",
        ApartmentNumber = "114A"
      };
      var printerAckResponse = new[] { FiscalPrinterResponses.Ack };
      MockSinglePrinterResponse(printerAckResponse);

      var response = await  Run<SetPosAddressCommand, SetPosAddressRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      Assert(() => fullCommandString == expectedCommandString);
      Assert(() => response.Success);
    }

    [Fact]
    public async Task Printer_Set_Pos_Address_Error()
    {
      ResetPrinterWriteRawDataResponse();
      var request = new SetPosAddressRequest
      {
        Place = "Valid Place",
        TaxOffice = "Valid TaxOffice",
        Street = "Valid Street",
        PostalCode = "112254",
        HouseNumber = "115",
        ApartmentNumber = "114A"
      };
      var printerAckResponse = new[] { FiscalPrinterResponses.Nak };
      MockSinglePrinterResponse(printerAckResponse);

      var response = await  Run<SetPosAddressCommand, SetPosAddressRequest>(request);
      Assert(() => !response.Success);
    }

    [Fact]
    public async Task Printer_Set_Pos_Address_Success_With_Empty_Place()
    {
      await Printer_Set_Pos_Address_Success_With_EmptyProperties(ComposeValidRequestWithout(nameof(SetPosAddressRequest.Place)));
    }

    [Fact]
    public async Task Printer_Set_Pos_Address_Success_With_Empty_TaxOffice()
    {
      await Printer_Set_Pos_Address_Success_With_EmptyProperties(ComposeValidRequestWithout(nameof(SetPosAddressRequest.TaxOffice)));
    }

    [Fact]
    public async Task Printer_Set_Pos_Address_Success_With_Empty_Street()
    {
      await Printer_Set_Pos_Address_Success_With_EmptyProperties(ComposeValidRequestWithout(nameof(SetPosAddressRequest.Street)));
    }

    [Fact]
    public async Task Printer_Set_Pos_Address_Success_With_Empty_PostalCode()
    {
      await Printer_Set_Pos_Address_Success_With_EmptyProperties(ComposeValidRequestWithout(nameof(SetPosAddressRequest.PostalCode)));
    }

    [Fact]
    public async Task Printer_Set_Pos_Address_Success_With_Empty_HouseNumber()
    {
      await Printer_Set_Pos_Address_Success_With_EmptyProperties(ComposeValidRequestWithout(nameof(SetPosAddressRequest.HouseNumber)));
    }

    [Fact]
    public async Task Printer_Set_Pos_Address_Success_With_Empty_ApartmentNumber()
    {
      await Printer_Set_Pos_Address_Success_With_EmptyProperties(ComposeValidRequestWithout(nameof(SetPosAddressRequest.ApartmentNumber)));
    }

    private async Task Printer_Set_Pos_Address_Success_With_EmptyProperties(SetPosAddressRequest request)
    {
      ResetPrinterWriteRawDataResponse();
      var printerAckResponse = new[] { FiscalPrinterResponses.Ack };
      MockSinglePrinterResponse(printerAckResponse);

      var response = await  Run<SetPosAddressCommand, SetPosAddressRequest>(request);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      Assert(() => fullCommandString == expectedCommandString);
      Assert(() => response.Success);
    }

    private static SetPosAddressRequest ComposeValidRequestWithout(string propertyName)
    {
      var validRequestSample = new SetPosAddressRequest
      {
        Place = "Valid Place",
        TaxOffice = "Valid TaxOffice",
        Street = "Valid Street",
        PostalCode = "112254",
        HouseNumber = "115",
        ApartmentNumber = "114A"
      };

      var result = new SetPosAddressRequest();

      if (propertyName != nameof(SetPosAddressRequest.Place))
      {
        result.Place = validRequestSample.Place;
      }

      if (propertyName != nameof(SetPosAddressRequest.TaxOffice))
      {
        result.TaxOffice = validRequestSample.TaxOffice;
      }

      if (propertyName != nameof(SetPosAddressRequest.Street))
      {
        result.Street = validRequestSample.Street;
      }

      if (propertyName != nameof(SetPosAddressRequest.PostalCode))
      {
        result.PostalCode = validRequestSample.PostalCode;
      }

      if (propertyName != nameof(SetPosAddressRequest.HouseNumber))
      {
        result.HouseNumber = validRequestSample.HouseNumber;
      }

      if (propertyName != nameof(SetPosAddressRequest.ApartmentNumber))
      {
        result.ApartmentNumber = validRequestSample.ApartmentNumber;
      }

      return result;
    }

    private static IEnumerable<byte> GetExpectedSentCommand(SetPosAddressRequest request)
    {
      var encodedPosAddressLines = PosAddressHelper.GetPosAddressEncoded(request, EncodingHelper.Encode);

      var sentCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.A
      };

      foreach (var encodedItem in encodedPosAddressLines)
      {
        sentCommand.AddRange(encodedItem);
        sentCommand.Add(FiscalPrinterDividers.Lf);
      }

      sentCommand.AddRange(new[] { FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe });

      return sentCommand;
    }
  }
}