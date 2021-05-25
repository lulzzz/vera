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
  public class DeleteSingleGraphicTests : FiscalPrinterCommandTestsBase
  {
    private readonly DeleteSingleGraphicRequest _sampleRequest = new()
    {
      Graphic = GraphicNumber.Graphic1
    };

    [Fact]
    public async Task Will_Validate_Input()
    {
      var request = new DeleteSingleGraphicRequest
      {
        Graphic = null
      };

      var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
      {
        await Run<DeleteSingleGraphicCommand, DeleteSingleGraphicRequest>(request);
      });

      Assert.Equal(nameof(ReadGraphicChecksumRequest.Graphic), exception.ParamName);
    }

    [Fact]
    public async Task DeleteSingleGraphicCommand_Works()
    {
      SetupAckRespondingPrinter();
      var response = await Run<DeleteSingleGraphicCommand, DeleteSingleGraphicRequest>(_sampleRequest);
      Assert.True(response.Success);
    }

    [Fact]
    public async Task DeleteSingleGraphicCommand_Requires_Request()
    {
      SetupAckRespondingPrinter();
      await AssertException<NullReferenceException>(null);
    }

    private async Task AssertException<T>(DeleteSingleGraphicRequest request)
      where T : Exception
    {
      SetupAckRespondingPrinter();
      var exception = await Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<DeleteSingleGraphicCommand, DeleteSingleGraphicRequest>(request);
      });

      Assert(() => exception.Message != null);
    }
  }
}