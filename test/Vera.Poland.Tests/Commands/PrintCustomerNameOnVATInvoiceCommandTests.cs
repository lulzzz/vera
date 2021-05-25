using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands
{
  public class PrintCustomerNameOnVatInvoiceCommandTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task When_Customer_Order_Line_Has_Lines_Too_Long_Will_Throw()
    {
      ResetPrinterWriteRawDataResponse();
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });

      var request = new PrintCustomerNameOnVatInvoiceRequest
      {
        CustomerNameLines = new List<string>
        {
          "Test1", "Test2", "TooLongTooLongTooLongTooLongTooLongTooLongTooLongTooLong", "Test4"
        }
      };
      
      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<PrintCustomerNameOnVatInvoiceCommand, PrintCustomerNameOnVatInvoiceRequest>(request);
      });

      Assert.Equal(nameof(PrintCustomerNameOnVatInvoiceRequest.CustomerNameLines), exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task If_Customer_Name_Lines_Contain_Null_Or_Whitespace_String_Will_Skip(string testCustomerNameLine)
    {
      ResetPrinterWriteRawDataResponse();
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });

      var request = new PrintCustomerNameOnVatInvoiceRequest
      {
        CustomerNameLines = new List<string>
        {
          "Test1", "Test2", testCustomerNameLine, "Test4"
        }
      };
      
      var response = await  Run<PrintCustomerNameOnVatInvoiceCommand, PrintCustomerNameOnVatInvoiceRequest>(request);

      Assert.True(response.Success);

      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.i
      };

      var expectedCustomerNameLines = new List<string>
      {
        "Test1", "Test2", "Test4"
      };
      for (var i = 0; i < expectedCustomerNameLines.Count; i++)
      {
        expectedCommand.AddRange(EncodingHelper.Encode(expectedCustomerNameLines[i]));
        if (i + 1 != expectedCustomerNameLines.Count)
        {
          expectedCommand.Add(FiscalPrinterDividers.Lf);
        }
      }
      expectedCommand.Add(FiscalPrinterCommands.Esc);
      expectedCommand.Add(FiscalPrinterCommands.Mfe);

      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());
      Assert.Equal(expectedCommandString, fullCommandString);
    }

    [Fact]
    public async Task Will_Propagate_Printer_Error()
    {
      ResetPrinterWriteRawDataResponse();
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Nak });

      var request = new PrintCustomerNameOnVatInvoiceRequest
      {
        CustomerNameLines = new List<string>
        {
          "Test1", "Test2", "Test3", "Test4"
        }
      };
      
      var response = await  Run<PrintCustomerNameOnVatInvoiceCommand, PrintCustomerNameOnVatInvoiceRequest>(request);

      Assert.False(response.Success);
    }
  }
}