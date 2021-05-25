using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vera.Poland.Commands.Invoice;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;
using Xunit;

namespace Vera.Poland.Tests.Commands.Invoice
{
  public class MutateReceiptPaymentAndFooterCommandTests : FiscalPrinterCommandTestsBase
  {
    [Fact]
    public async Task When_Index_Is_Footer_h_Will_Throw()
    {
      await Assert.ThrowsAsync<InvalidOperationException>(async () =>
      {
        await  Run<MutateReceiptPaymentAndFooterCommand, MutateReceiptPaymentAndFooterRequest>(
          BuildRequest(SupportedPaymentAndFooterTypes.h, null));
      });
    }

    [Fact]
    public async Task Will_Validate_Number_Of_Parameters()
    {
      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<MutateReceiptPaymentAndFooterCommand, MutateReceiptPaymentAndFooterRequest>(
          BuildRequest(SupportedPaymentAndFooterTypes.A, new List<ParameterModel>
          {
            new(),
            new()
          }));
      });

      Assert.Equal(nameof(MutateReceiptPaymentAndFooterRequest.Index), exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Will_Validate_Parameter_Name(string name)
    {
      var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
      {
        await  Run<MutateReceiptPaymentAndFooterCommand, MutateReceiptPaymentAndFooterRequest>(
          BuildRequest(SupportedPaymentAndFooterTypes.A, BuildParameterModels(name: name)));
      });

      Assert.Equal(nameof(ParameterModel.Name), exception.ParamName);
    }

    [Theory]
    [InlineData(ParameterTypeEnum.Ignored)]
    [InlineData(ParameterTypeEnum.Type2)]
    [InlineData(ParameterTypeEnum.Type3)]
    [InlineData(ParameterTypeEnum.Type5Q)]
    [InlineData(ParameterTypeEnum.Type6Q)]
    public async Task Will_Validate_Parameter_Type(ParameterTypeEnum type)
    {
      await Assert.ThrowsAsync<InvalidOperationException>(async () =>
      {
        await  Run<MutateReceiptPaymentAndFooterCommand, MutateReceiptPaymentAndFooterRequest>(
          BuildRequest(SupportedPaymentAndFooterTypes.A, BuildParameterModels(type: type)));
      });
    }

    // No known cases
    //[Theory]
    //[InlineData("12345678901234567890123456789012345678901234567890123456")]
    //[InlineData("123456789012345678901234567890123456789012345678901234567")]
    //public async Task Will_Validate_ParamType1_Length(string name)
    //{
    //  var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
    //  {
    //    await  Run<MutateReceiptPaymentAndFooterCommand, MutateReceiptPaymentAndFooterRequest>(
    //      BuildRequest("", BuildParameterModels(name: name)));
    //  });

    //  Assert.Equal(nameof(ParameterModel.Name), exception.ParamName);
    //}

    [Fact]
    public async Task Will_Validate_ParamType2_Length()
    {
      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<MutateReceiptPaymentAndFooterCommand, MutateReceiptPaymentAndFooterRequest>(
          BuildRequest(SupportedPaymentAndFooterTypes.A, BuildParameterModels(name: "1234567890123")));
      });

      Assert.Equal(nameof(ParameterModel.Name), exception.ParamName);
    }

    [Fact]
    public async Task Will_Validate_ParamType2_Comma_Split()
    {
      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<MutateReceiptPaymentAndFooterCommand, MutateReceiptPaymentAndFooterRequest>(
          BuildRequest(SupportedPaymentAndFooterTypes.A, BuildParameterModels(name: "12345678,1")));
      });

      Assert.Equal(nameof(ParameterModel.Name), exception.ParamName);
    }

    [Fact]
    public async Task Will_Validate_ParamType5Q_Length()
    {
      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<MutateReceiptPaymentAndFooterCommand, MutateReceiptPaymentAndFooterRequest>(
          BuildRequest(SupportedPaymentAndFooterTypes.Q, new List<ParameterModel>
          {
            new()
            {
              Name = "NotExactly18Characters",
              Type = ParameterTypeEnum.Type5Q
            },
            new()
            {
              Name = "TestName",
              Type = ParameterTypeEnum.Type6Q
            },
          }));
      });
    }

    [Fact]
    public async Task Will_Validate_ParamType6Q_Length()
    {
      var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
      {
        await  Run<MutateReceiptPaymentAndFooterCommand, MutateReceiptPaymentAndFooterRequest>(
          BuildRequest(SupportedPaymentAndFooterTypes.Q, new List<ParameterModel>
          {
              new()
              {
                Name = "123456789012345678",
                Type = ParameterTypeEnum.Type5Q
              },
              new()
              {
                Name = "More_Than_18_Characters_For_Sure",
                Type = ParameterTypeEnum.Type6Q
              },
          }));
      });

      Assert.Equal(nameof(ParameterModel.Name), exception.ParamName);
    }

    [Fact]
    public async Task Will_Not_Validate_Type_3()
    {
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });
      var response = await  Run<MutateReceiptPaymentAndFooterCommand, MutateReceiptPaymentAndFooterRequest>(
        BuildRequest(SupportedPaymentAndFooterTypes.B, BuildParameterModels(type: ParameterTypeEnum.Type3)));

      Assert.True(response.Success);
    }

    [Fact]
    public async Task Will_Not_Validate_Type_Ignored()
    {
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });
      var response = await  Run<MutateReceiptPaymentAndFooterCommand, MutateReceiptPaymentAndFooterRequest>(
        BuildRequest(SupportedPaymentAndFooterTypes.Two_2, BuildParameterModels(type: ParameterTypeEnum.Ignored)));

      Assert.True(response.Success);
    }

    [Fact]
    public async Task Will_Send_Correct_Command_To_Printer()
    {
      MockExactPrinterResponse(new[] { FiscalPrinterResponses.Ack });

      var request = BuildRequest(SupportedPaymentAndFooterTypes.Q, new List<ParameterModel>
      {
        new()
        {
          Name = "123456789012345678",
          Type = ParameterTypeEnum.Type5Q
        },
        new()
        {
          Name = "1234567",
          Type = ParameterTypeEnum.Type6Q
        },
      });

      var response = await  Run<MutateReceiptPaymentAndFooterCommand, MutateReceiptPaymentAndFooterRequest>(request);

      Assert.True(response.Success);

      GetExpectedCommand(request);
    }

    private void GetExpectedCommand(MutateReceiptPaymentAndFooterRequest request)
    {
      var fullCommandString = EncodingHelper.Decode(CommandPayload.ToArray());
      var expectedCommand = new List<byte>
      {
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
      };
      expectedCommand.Add(FiscalPrinterDividers.R);
      expectedCommand.AddRange(EncodingHelper.Encode(request.Index.ToString()));

      for (var i = 0; i < request.Parameters.Count; i++)
      {
        if (i != 0)
        {
          expectedCommand.Add(FiscalPrinterDividers.Lf);
        }
        expectedCommand.AddRange(EncodingHelper.Encode(request.Parameters[i].Name));
      }

      expectedCommand.Add(FiscalPrinterCommands.Esc);
      expectedCommand.Add(FiscalPrinterCommands.Mfe);

      var expectedCommandString = EncodingHelper.Decode(expectedCommand.ToArray());
      Assert.Equal(expectedCommandString, fullCommandString);
    }

    private static MutateReceiptPaymentAndFooterRequest BuildRequest(SupportedPaymentAndFooterTypes index, List<ParameterModel> parameterModels)
    {
      return new()
      {
        Index = index,
        Parameters = parameterModels
      };
    }

    private static List<ParameterModel> BuildParameterModels(string name = "TestName", ParameterTypeEnum type = ParameterTypeEnum.Type2)
    {
      return new()
      {
        new ParameterModel
        {
          Name = name,
          Type = type
        }
      };
    }
  }
}