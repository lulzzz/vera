using System;
using System.Net.Http;
using System.Threading.Tasks;
using EVA.Auditing.Sweden.Models;
using EVA.Auditing.Sweden.Models.Responses;
using Newtonsoft.Json;
using Vera.Sweden.Models.Exceptions;
using Vera.Sweden.Validators.Contracts;

namespace Vera.Sweden.Validators
{
  public class InfrasecEnrollmentResponseValidator : IInfrasecEnrollmentResponseValidator
  {
    // TODO(SEBI): private readonly ILog _log = LogProvider.GetLogger(nameof(InfrasecEnrollmentResponseValidator));

    public async Task<InfrasecEnrollmentResponse> HandleInfrasecResponse(HttpResponseMessage response, string action, Guid stationID)
    {
      var responseContent = await response.Content.ReadAsStringAsync();

      var infrasecResponseModel = JsonConvert.DeserializeObject<InfrasecEnrollmentResponse>(responseContent);

      if (!response.IsSuccessStatusCode || infrasecResponseModel.IdmResponse?.ResponseMessage != "Success")
      {
        var explicitErrorMessage = $"Infrasec enrollment action = {action} failed for station with ID: {stationID}";
        HandleErrorResponse(explicitErrorMessage, responseContent, infrasecResponseModel);
      }

      return infrasecResponseModel;
    }

    private void HandleErrorResponse(string explicitErrorMessage, string responseContent, InfrasecEnrollmentResponse infrasecResponseModel)
    {
      explicitErrorMessage += responseContent;
      // TODO(SEBI): _log.Error(explicitErrorMessage);

      var responseCode = infrasecResponseModel.IdmResponse?.ResponseCode;
      // Throw more explicit exceptions from common Infrasec errors
      if (!string.IsNullOrWhiteSpace(responseCode))
      {
        switch (responseCode)
        {
          // RegisterID <X> already exists Error
          case InfrasecKnownErrorCodes.InfrasecEnrollmentApiRegisterAlreadyEnrolled:
          {
            throw new RegisterWithThisIdAlreadyEnrolled($"Infrasec Unique register identity [{infrasecResponseModel.IdmResponse?.RegisterID}] already used. " +
                                                        $"Either this register was initialized already with Infrasec or a different one with the same combination TenantCode, ShopNumber, StationNumber etc. was already registered." +
                                                        $"{explicitErrorMessage}");
          }
        }
      }
      throw new EnrollmentFailed(explicitErrorMessage);
    }
  }
}