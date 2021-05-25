using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assertive;
using Vera.Poland.Commands;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;
using Xunit;
using Assert = Xunit.Assert;

namespace Vera.Poland.Tests.Commands
{
  public class SetPaymentTypesCommandTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task Printer_Set_Payment_Types_Success()
    {
      ResetPrinterWriteRawDataResponse();
      var printerAckResponse = new[] { FiscalPrinterResponses.Ack };
      MockSinglePrinterResponse(printerAckResponse);

      var request = new SetPaymentTypeRequest
      {
        PaymentType = "VISA"
      };

      var response = await  Run<SetPaymentTypeCommand, SetPaymentTypeRequest>(request);

      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = GetExpectedSentCommand(request.PaymentType);
      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());

      DSL.Assert(() => fullCommandString == expectedCommandString);
      DSL.Assert(() => response.Success);
    }

    [Fact]
    public async Task Printer_Set_Payment_Types_Payload_Too_Long()
    {
      ResetPrinterWriteRawDataResponse();
      var printerAckResponse = new[] { FiscalPrinterResponses.Ack };
      MockSinglePrinterResponse(printerAckResponse);

      var request = new SetPaymentTypeRequest
      {
        PaymentType = "Some super long payment type which is greater than 18 characters"
      };

      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<SetPaymentTypeCommand, SetPaymentTypeRequest>(request);
      });

      Assert.Equal(nameof(SetPaymentTypeRequest.PaymentType), exception.ParamName);
    }

    [Fact]
    public async Task Printer_Set_Payment_Types_Payload_Empty()
    {
      ResetPrinterWriteRawDataResponse();
      var printerAckResponse = new[] { FiscalPrinterResponses.Ack };
      MockSinglePrinterResponse(printerAckResponse);

      var request = new SetPaymentTypeRequest
      {
        PaymentType = ""
      };

      var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
      {
        await  Run<SetPaymentTypeCommand, SetPaymentTypeRequest>(request);
      });

      Assert.Equal(nameof(SetPaymentTypeRequest.PaymentType), exception.ParamName);
    }

    private static IEnumerable<byte> GetExpectedSentCommand(string paymentType)
    {
      var commandStartPart = new[]
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.P
      };

      var encodedPaymentType = EncodingHelper.Encode(paymentType);

      var commandEndPart = new[]
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe
      };

      var expectedSentCommand = commandStartPart.Concat(encodedPaymentType).Concat(commandEndPart).ToArray();

      return expectedSentCommand;
    }
  }
}