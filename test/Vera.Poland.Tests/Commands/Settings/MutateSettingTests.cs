using System;
using System.Threading.Tasks;
using Vera.Poland.Commands.Settings;
using Vera.Poland.Models.Requests.Settings;
using Xunit;
using static Assertive.DSL;

namespace Vera.Poland.Tests.Commands.Settings
{
  public class MutateSettingTests : FiscalPrinterCommandTestsBase
  {
    private readonly MutateSettingRequest _sampleRequest = new()
    {
      SettingName = "Name",
      SettingValue = "Value"
    };

    [Fact]
    public async Task MutateSettingCommand_Works()
    {
      SetupAckRespondingPrinter();
      var request = _sampleRequest;
      await TestSuccessfulOperation(request);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task MutateSettingCommand_SettingName_Mandatory(string settingName)
    {
      SetupAckRespondingPrinter();
      _sampleRequest.SettingName = settingName;
      await AssertArgumentException<ArgumentNullException>(_sampleRequest, nameof(_sampleRequest.SettingName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task MutateSettingCommand_SettingValue_Mandatory(string settingValue)
    {
      SetupAckRespondingPrinter();
      _sampleRequest.SettingValue = settingValue;
      await AssertArgumentException<ArgumentNullException>(_sampleRequest, nameof(_sampleRequest.SettingValue));
    }

    private async Task TestSuccessfulOperation(MutateSettingRequest request)
    {
      var response = await  Run<MutateSettingCommand, MutateSettingRequest>(request);
      Assert(() => response.Success);
    }

    private async Task AssertArgumentException<T>(MutateSettingRequest request, string paramName)
      where T : ArgumentException
    {
      SetupAckRespondingPrinter();

      var exception = await Xunit.Assert.ThrowsAsync<T>(async () =>
      {
        await  Run<MutateSettingCommand, MutateSettingRequest>(request);
      });

      Assert(() => exception.Message != null);
      Assert(() => exception.ParamName == paramName);
    }
  }
}