using Vera.Sweden.Models.Configs;
using Vera.Sweden.Models.Records;

namespace Vera.Sweden.Validators.Contracts
{
  public interface IInfrasecEnrollmentRequestValidator
  {
    void ValidateSettingFields(MandatoryEnrollmentSettingFields fieldsFromSettings);
    MandatoryEnrollmentSettingFields SanitizeSettingFields(MandatoryEnrollmentSettingFields fieldsFromSettings);
    string SanitizeStationNumber(int currentStationNumber);
    void ValidateSanitizedFields(MandatoryEnrollmentSettingFields fieldsFromSettings);
    void ValidateCurrentStationNumber(int currentStationNumber);
    void ValidateInfrasecRegisterID(string infrasecRegisterID);
    void ValidateSwedenAuditingConstants();
    void ValidateInfrasecEnrollmentData(InfrasecEnrollmentRecord infrasecNewOrUpdateEnrollmentData);
    void ValidateCompanyRegistrationNumber(string registrationNumber);
  }
}