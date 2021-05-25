using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vera.Poland.Commands;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;
using Xunit;
using static Assertive.DSL;
using Assert = Xunit.Assert;

#pragma warning disable 1998

namespace Vera.Poland.Tests.Commands
{
  public class SetCashierCommandTests: FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task Printer_Set_Cashier_Success()
    {
      ResetPrinterWriteRawDataResponse();
      var printerAckResponse = new[] { FiscalPrinterResponses.Ack };
      MockSinglePrinterResponse(printerAckResponse);

      var request = new SetCashierRequest
      {
        TerminalNumber = "XXX",
        CashierIdentifier = "Cashier"
      };

      var response = await  Run<SetCashierCommand, SetCashierRequest>(request);

      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetFullCashierSetCommand(request);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      Assert(() => fullCommandString == expectedCommandString);
      Assert(() => response.Success);
    }

    [Fact]
    public async Task Printer_Set_Cashier_Invalid_Terminal_Number()
    {
      var request = new SetCashierRequest
      {
        TerminalNumber = "XX",
        CashierIdentifier = "Some Cashier"
      };

      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<SetCashierCommand, SetCashierRequest>(request);
      });

      Assert.Equal(nameof(SetCashierRequest.TerminalNumber), exception.ParamName);
    }

    [Fact]
    public async Task Printer_Set_Cashier_Empty_Terminal_Number()
    {
      var request = new SetCashierRequest
      {
        TerminalNumber = "",
        CashierIdentifier = "Some Cashier"
      };

      var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
      {
        await  Run<SetCashierCommand, SetCashierRequest>(request);
      });

      Assert.Equal(nameof(SetCashierRequest.TerminalNumber), exception.ParamName);
    }

    [Fact]
    public async Task Printer_Set_Cashier_Empty_Cashier_Identifier()
    {
      var request = new SetCashierRequest
      {
        TerminalNumber = "XXX",
        CashierIdentifier = ""
      };

      var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
      {
        await  Run<SetCashierCommand, SetCashierRequest>(request);
      });

      Assert.Equal(nameof(SetCashierRequest.CashierIdentifier), exception.ParamName);
    }

    [Fact]
    public async Task Printer_Set_Cashier_Invalid_Cashier_Identifier()
    {
      var request = new SetCashierRequest
      {
        TerminalNumber = "XXX",
        CashierIdentifier = "Some Very Long Cashier Identifier"
      };

      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<SetCashierCommand, SetCashierRequest>(request);
      });

      Assert.Equal(nameof(SetCashierRequest.CashierIdentifier), exception.ParamName);
    }

    private static IEnumerable<byte> GetFullCashierSetCommand(SetCashierRequest request)
    {
      var startCommand = new[]
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.J,
      };

      var encodedPayload = EncodingHelper.Encode($"{request.TerminalNumber}{request.CashierIdentifier}");
      var endCommand = new[]
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe
      };

      var command = startCommand.Concat(encodedPayload).Concat(endCommand).ToArray();

      return command;
    }
  }
}
