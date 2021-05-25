using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Poland.Commands.HandleGraphics;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests.HandleGraphics;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.HandleGraphics
{
  public class InitializeGraphicLoadingCommandTests : FiscalPrinterCommandTestsBase
  {

    [Fact]
    public async Task Will_Validate_Graphic_Number()
    {
      var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
      {
        await  Run<InitializeGraphicLoadingCommand, InitializeGraphicLoadingRequest>(BuildRequest(graphicNumber: null));
      });

      Assert.Equal(nameof(InitializeGraphicLoadingRequest.Graphic), exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(361)]
    public async Task Will_Validate_Width(uint width)
    {
      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<InitializeGraphicLoadingCommand, InitializeGraphicLoadingRequest>(BuildRequest(width: width));
      });

      Assert.Equal(nameof(InitializeGraphicLoadingRequest.Width), exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(361)]
    public async Task Will_Validate_Height(uint height)
    {
      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<InitializeGraphicLoadingCommand, InitializeGraphicLoadingRequest>(BuildRequest(height: height));
      });

      Assert.Equal(nameof(InitializeGraphicLoadingRequest.Height), exception.ParamName);
    }

    [Fact]
    public async Task Will_Validate_Name_Length()
    {
      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<InitializeGraphicLoadingCommand, InitializeGraphicLoadingRequest>(BuildRequest(name: "NameIsTooLong1234"));
      });

      Assert.Equal(nameof(InitializeGraphicLoadingRequest.Name), exception.ParamName);
    }

    [Fact]
    public async Task Will_Send_Printer_Error_Status()
    {
      ResetPrinterWriteRawDataResponse();
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Nak });


      var result = await Run<InitializeGraphicLoadingCommand, InitializeGraphicLoadingRequest>(BuildRequest());
      Assert.False(result.Success);
    }

    [Theory]
    [InlineData(GraphicNumber.Graphic1, 1, true)]
    [InlineData(GraphicNumber.Graphic2, 2, true)]
    [InlineData(GraphicNumber.Graphic3, 3, true)]
    [InlineData(GraphicNumber.Graphic4, 4, true)]
    [InlineData(GraphicNumber.Graphic5, 5, true)]
    [InlineData(GraphicNumber.Graphic6, 6, true)]
    [InlineData(GraphicNumber.Graphic7, 7, true)]
    [InlineData(GraphicNumber.Graphic8, 8, true)]
    [InlineData(GraphicNumber.Graphic4, 4, false)]
    public async Task Will_Send_Correct_Command_To_The_Printer_Based_On_Name_And_Selected_Graphic(GraphicNumber graphicNumber, int expectedGraphicNumber, bool nameProvided)
    {
      ResetPrinterWriteRawDataResponse();
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Nak });
      var request = BuildRequest(name: nameProvided ? "NameTest" : null, graphicNumber: graphicNumber);
      var result = await Run<InitializeGraphicLoadingCommand, InitializeGraphicLoadingRequest>(BuildRequest(name: nameProvided ? "NameTest" : null, graphicNumber: graphicNumber));

      Assert.False(result.Success);

      GetExpectedCommand(request, nameProvided, expectedGraphicNumber);
    }

    private void GetExpectedCommand(InitializeGraphicLoadingRequest request, bool nameProvided, int expectedGraphicNumber)
    {
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.OpenParenthesis
      };
      expectedCommand.AddRange(FiscalPrinterDividers.LB);
      expectedCommand.AddRange(EncodingHelper.Encode(expectedGraphicNumber));
      expectedCommand.Add(FiscalPrinterDividers.Lf);
      expectedCommand.AddRange(EncodingHelper.Encode(request.Colour));
      expectedCommand.Add(FiscalPrinterDividers.Lf);
      expectedCommand.AddRange(EncodingHelper.Encode(request.Width));
      expectedCommand.Add(FiscalPrinterDividers.Lf);
      expectedCommand.AddRange(EncodingHelper.Encode(request.Height));
      if (nameProvided)
      {
        expectedCommand.Add(FiscalPrinterDividers.Lf);
        expectedCommand.AddRange(EncodingHelper.Encode(request.Name));
      }
      expectedCommand.Add(FiscalPrinterCommands.Esc);
      expectedCommand.Add(FiscalPrinterCommands.Mfe);

      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());
      Assert.Equal(expectedCommandString, fullCommandString);
    }

    private static InitializeGraphicLoadingRequest BuildRequest(uint height = 100, uint width = 50, string name = "Test", GraphicNumber? graphicNumber = GraphicNumber.Graphic1)
    {
      var request = new InitializeGraphicLoadingRequest
      {
        Width = width,
        Height = height,
        Name = name,
        Graphic = graphicNumber
      };
      return request;
    }
  }
}