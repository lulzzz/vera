using System;
using System.Threading.Tasks;
using Vera.Poland.Commands.HandleGraphics;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests.HandleGraphics;
using Xunit;
using Assert = Xunit.Assert;
using static Assertive.DSL;

namespace Vera.Poland.Tests.Commands.HandleGraphics
{
  public class PrintGraphicTests : FiscalPrinterCommandTestsBase
  {
    private readonly PrintGraphicRequest _sampleRequest = new()
    {
      Graphic = GraphicNumber.Graphic1
    };

    [Fact]
    public async Task Will_Validate_Input()
    {
      var request = new PrintGraphicRequest
      {
        Graphic = null
      };

      var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
      {
        await  Run<PrintGraphicCommand, PrintGraphicRequest>(request);
      });

      Assert.Equal(nameof(ReadGraphicChecksumRequest.Graphic), exception.ParamName);
    }

    [Fact]
    public async Task PrintGraphicCommand_Works()
    {
      SetupAckRespondingPrinter();

      var response = await  Run<PrintGraphicCommand, PrintGraphicRequest>(_sampleRequest);
      Assert.True(response.Success);
    }

    [Fact]
    public async Task PrintGraphicCommand_Requires_Request()
    {
      SetupAckRespondingPrinter();
      await AssertException<NullReferenceException>(null);
    }

    private async Task AssertException<T>(PrintGraphicRequest request)
      where T : Exception
    {
      SetupAckRespondingPrinter();
      var exception = await Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<PrintGraphicCommand, PrintGraphicRequest>(request);
      });

      Assert(() => exception.Message != null);
    }
  }
}