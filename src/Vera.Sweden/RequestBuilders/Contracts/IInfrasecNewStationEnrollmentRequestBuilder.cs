using Vera.Sweden.Models.Configs;
using Vera.Sweden.Models.Records;
using Vera.Sweden.Models.Requests.EnrollmentApi;

namespace Vera.Sweden.RequestBuilders.Contracts
{
  public interface IInfrasecNewStationEnrollmentRequestBuilder
  {
    InfrasecNewOrUpdateEnrollmentRequest BuildRequest(MandatoryEnrollmentSettingFields fieldsFromSettings, InfrasecEnrollmentRecord infrasecEnrollmentData, int currentStationNumber);
  }
}