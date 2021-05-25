using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vera.Poland.Commands.HandleGraphics;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests.HandleGraphics;
using Vera.Poland.Models.Responses.HandleGraphics;
using Vera.Poland.Protocol;
using Xunit;
using Assert = Xunit.Assert;
using static Assertive.DSL;

namespace Vera.Poland.Tests.Commands.HandleGraphics
{
  public class ReadGraphicChecksumTests : FiscalPrinterCommandTestsBase
  {
    private readonly List<byte> _printerFullResponse = new()
    {
      FiscalPrinterCommands.Esc,
      FiscalPrinterResponses.ResponseArgument,
      1,
      2,
      3,
      4,
      5,
      6,
      7,
      8
    };

    [Fact]
    public async Task Will_Validate_Input()
    {
      var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
      {
        await  Run<ReadGraphicChecksumQuery, ReadGraphicChecksumRequest, ReadGraphicChecksumResponse>(BuildRequest(null));
      });

      Assert.Equal(nameof(ReadGraphicChecksumRequest.Graphic), exception.ParamName);
    }

    [Fact]
    public async Task Will_Return_Checksum()
    {
      ResetPrinterWriteRawDataResponse();
      MockAllPrinterResponses(_printerFullResponse.ToArray());
      
      var response = await  Run<ReadGraphicChecksumQuery, ReadGraphicChecksumRequest, ReadGraphicChecksumResponse>(
        BuildRequest(GraphicNumber.Graphic3));

      var expectedChecksum = EncodingHelper.Decode(_printerFullResponse.Skip(2).ToArray());

      Assert(() => response.Checksum == expectedChecksum);
      Assert(() => response.Success);
    }

    [Theory]
    [InlineData(GraphicNumber.Graphic1)]
    [InlineData(GraphicNumber.Graphic7)]
    public async Task Will_Send_Correct_Command(GraphicNumber graphicNumber)
    {
      ResetPrinterWriteRawDataResponse();
      MockAllPrinterResponses(_printerFullResponse.ToArray());
      var response = await  Run<ReadGraphicChecksumQuery, ReadGraphicChecksumRequest, ReadGraphicChecksumResponse>(
        BuildRequest(graphicNumber));

      var expectedCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.OpenParenthesis, FiscalPrinterDividers.R

      };
      var decodedExpectedCommand = GetExpectedSentCommand(expectedCommand, (int)graphicNumber);
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());

      Assert(() => fullCommandString == decodedExpectedCommand);
      Assert(() => response.Success);
    }

    private static ReadGraphicChecksumRequest BuildRequest(GraphicNumber? graphicNumber)
    {
      return new()
      {
        Graphic = graphicNumber
      };
    }

    private static string GetExpectedSentCommand(List<byte> expectedCommand, int expectedGraphic)
    {
      expectedCommand.AddRange(EncodingHelper.Encode(expectedGraphic));
      expectedCommand.Add(FiscalPrinterCommands.Esc);
      expectedCommand.Add(FiscalPrinterCommands.Mfe);

      var decodedExpectedCommand = EncodingHelper.Decode(expectedCommand.ToArray());
      return decodedExpectedCommand;
    }
  }
}