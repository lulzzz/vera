using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EVA.Auditing.Sweden.Models;
using EVA.Auditing.Sweden.Models.Responses;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Newtonsoft.Json;
using Vera.Sweden.Models.Enums;
using Vera.Sweden.Models.Exceptions;
using Vera.Sweden.Validators;
using Vera.Sweden.Validators.Contracts;
using Xunit;

namespace Vera.Sweden.Tests.Validators
{
  public class InfrasecEnrollmentResponseValidatorTests
  {
    private readonly IInfrasecEnrollmentResponseValidator _infrasecEnrollmentResponseValidator;
    private readonly Guid _stationId = new Guid("CAB8A6DE-2F0C-4781-961F-F4DF2FF32831");

    public InfrasecEnrollmentResponseValidatorTests()
    {
      _infrasecEnrollmentResponseValidator = new InfrasecEnrollmentResponseValidator();
    }

    // HandleInfrasecResponse
    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.BadRequest)]
    public async Task When_Response_Is_Known_Error_86_Will_Throw(HttpStatusCode httpStatusCode)
    {
      var infrasecEnrollmentResponseModel = new InfrasecEnrollmentResponse
      {
        IdmResponse = BuildInfrasecResponseModel(responseMessage: "Failed", registerID: "", responseCode: InfrasecKnownErrorCodes.InfrasecEnrollmentApiRegisterAlreadyEnrolled)
      };
      var infrasecResponse = BuildInfrasecHttpResponse(infrasecEnrollmentResponseModel, httpStatusCode);

      var exception = await Assert.ThrowsAsync<RegisterWithThisIdAlreadyEnrolled>(async () =>
      {
        await _infrasecEnrollmentResponseValidator.HandleInfrasecResponse(infrasecResponse, AvailableInfrasecRequestActions.NEW.ToString(), _stationId);
      });

      Assert.Contains(_stationId.ToString(), exception.Message);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.BadRequest)]
    public async Task When_Response_Is_Unspecified_Error_Will_Throw(HttpStatusCode httpStatusCode)
    {
      var infrasecEnrollmentResponseModel = new InfrasecEnrollmentResponse
      {
        IdmResponse = BuildInfrasecResponseModel(responseMessage: "Failed", registerID: "", responseCode: "999")
      };
      var infrasecResponse = BuildInfrasecHttpResponse(infrasecEnrollmentResponseModel, httpStatusCode);

      var exception = await Assert.ThrowsAsync<EnrollmentFailed>(async () =>
      {
        await _infrasecEnrollmentResponseValidator.HandleInfrasecResponse(infrasecResponse, AvailableInfrasecRequestActions.NEW.ToString(), _stationId);
      });

      Assert.Contains(_stationId.ToString(), exception.Message);
    }

    [Fact]
    public async Task Will_Handle_Successful_Infrasec_Response()
    {
      var infrasecEnrollmentResponseModel = new InfrasecEnrollmentResponse
      {
        IdmResponse = BuildInfrasecResponseModel()
      };
      var infrasecResponse = BuildInfrasecHttpResponse(infrasecEnrollmentResponseModel);

      var result = await _infrasecEnrollmentResponseValidator.HandleInfrasecResponse(infrasecResponse, AvailableInfrasecRequestActions.NEW.ToString(),
        _stationId);

      Assert.Equal(infrasecEnrollmentResponseModel.IdmResponse.ResponseMessage, result.IdmResponse.ResponseMessage);
      Assert.Equal(infrasecEnrollmentResponseModel.IdmResponse.RegisterID, result.IdmResponse.RegisterID);
    }

    private static HttpResponseMessage BuildInfrasecHttpResponse(InfrasecEnrollmentResponse infrasecEnrollmentResponseModel,
      HttpStatusCode httpStatusCode = HttpStatusCode.OK)
    {
      return new()
      {
        StatusCode = httpStatusCode,
        Content = new StringContent(JsonConvert.SerializeObject(infrasecEnrollmentResponseModel))
      };
    }

    private static InfrasecEnrollmentResponse.IdmResponseData BuildInfrasecResponseModel(
      string responseMessage = "Success", string registerID = "RegisterID", string responseCode = "")
    {
      return new()
      {
        ResponseMessage = responseMessage,
        RegisterID = registerID,
        ResponseCode = responseCode
      };
    }
  }
}