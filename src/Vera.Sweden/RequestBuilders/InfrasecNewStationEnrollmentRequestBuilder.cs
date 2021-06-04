using System;
using Vera.Sweden.Models.Configs;
using Vera.Sweden.Models.Constants;
using Vera.Sweden.Models.Enums;
using Vera.Sweden.Models.Records;
using Vera.Sweden.Models.Requests.EnrollmentApi;
using Vera.Sweden.RequestBuilders.Contracts;
using Vera.Sweden.Validators.Contracts;

namespace Vera.Sweden.RequestBuilders
{
  public class InfrasecNewStationEnrollmentRequestBuilder : IInfrasecNewStationEnrollmentRequestBuilder
  {
    private readonly IInfrasecEnrollmentRequestValidator _infrasecEnrollmentValidator;

    public InfrasecNewStationEnrollmentRequestBuilder(IInfrasecEnrollmentRequestValidator infrasecEnrollmentValidator)
    {
      _infrasecEnrollmentValidator = infrasecEnrollmentValidator;
    }

    private const string EnableCCU = "Yes";
    private const string EnableSwish = "No";
    private const string EnableSparakvittot = "No";

    public InfrasecNewOrUpdateEnrollmentRequest BuildRequest(MandatoryEnrollmentSettingFields fieldsFromSettings,
      InfrasecEnrollmentRecord infrasecEnrollmentData, int currentStationNumber)
    {
      _infrasecEnrollmentValidator.ValidateCompanyRegistrationNumber(infrasecEnrollmentData.CompanyRegistrationNumber);
      _infrasecEnrollmentValidator.ValidateSwedenAuditingConstants();
      _infrasecEnrollmentValidator.ValidateInfrasecEnrollmentData(infrasecEnrollmentData);
      _infrasecEnrollmentValidator.ValidateCurrentStationNumber(currentStationNumber);
      _infrasecEnrollmentValidator.ValidateSettingFields(fieldsFromSettings);
      _infrasecEnrollmentValidator.SanitizeSettingFields(fieldsFromSettings);
      _infrasecEnrollmentValidator.ValidateSanitizedFields(fieldsFromSettings);
      
      var sanitizeStationNumber = _infrasecEnrollmentValidator.SanitizeStationNumber(currentStationNumber);  // used to compute the RegisterID
      var requestID = Guid.NewGuid().ToString();

      return new InfrasecNewOrUpdateEnrollmentRequest
      {
        IdmRequest = new InfrasecNewOrUpdateEnrollmentRequest.IdmRequestData
        {
          ApplicationID = SwedenAuditingConstants.InfrasecApplicationID, // Optional
          RequestID = requestID,
          EnrollData = new InfrasecNewOrUpdateEnrollmentRequest.EnrollData
          {
            Action = AvailableInfrasecRequestActions.NEW.ToString(),
            PartnerAuthority = new InfrasecNewOrUpdateEnrollmentRequest.PartnerAuthority
            {
              PartnerCode = SwedenAuditingConstants.InfrasecApiPartnerCode, // Mandatory
              PartnerName = SwedenAuditingConstants.InfrasecApiPartnerName, // Mandatory
              POSAuthorityCode = fieldsFromSettings.InfrasecApiPosAuthorityCode, // Mandatory
            },
            OrganizationChain = new InfrasecNewOrUpdateEnrollmentRequest.OrganizationChain
            {
              ChainName = fieldsFromSettings.TenantName,
              ChainCode = fieldsFromSettings.TenantCode
            },
            StoreInfo = new InfrasecNewOrUpdateEnrollmentRequest.StoreInfo
            {
              StoreID = fieldsFromSettings.ShopNumber, // Mandatory: NEW
              StoreName = infrasecEnrollmentData.OrganizationUnitName, // Mandatory: NEW
              Address = infrasecEnrollmentData.OrganizationUnitAddress, // Mandatory: NEW
              City = infrasecEnrollmentData.OrganizationUnitCity, // Mandatory: NEW
              Zipcode = infrasecEnrollmentData.OrganizationUnitZipCode, // Mandatory: NEW,
              Email = infrasecEnrollmentData.OrganizationUnitEmail // Optional
            },
            CompanyInfo = new InfrasecNewOrUpdateEnrollmentRequest.CompanyInfo
            {
              Zipcode = infrasecEnrollmentData.CompanyZipcode, // Mandatory: NEW
              City = infrasecEnrollmentData.CompanyCity, // Mandatory: NEW
              Address = infrasecEnrollmentData.CompanyAddress, // Mandatory: NEW
              Company = infrasecEnrollmentData.CompanyName, // Mandatory: NEW
              OrganizationNumber = infrasecEnrollmentData.CompanyRegistrationNumber // Mandatory: NEW
            },
            RegisterInfo = new InfrasecNewOrUpdateEnrollmentRequest.RegisterInfo
            {
              RegisterID = "", // Computed by Infrasec, Mandatory: CHANGE, OPEN, CLOSE, STATUS
              RegisterMake = infrasecEnrollmentData.StationName, // Mandatory: NEW
              RegisterModel = infrasecEnrollmentData.StationName, // Mandatory: NEW
              Address = infrasecEnrollmentData.OrganizationUnitAddress, // Mandatory: NEW
              Zipcode = infrasecEnrollmentData.OrganizationUnitZipCode, // Mandatory: NEW
              City = infrasecEnrollmentData.OrganizationUnitCity, // Mandatory: NEW
              CounterNumber = sanitizeStationNumber, // Optional in theory, it's the current station number
            },
            JournalLocation = new InfrasecNewOrUpdateEnrollmentRequest.JournalLocation
            {
              Address = infrasecEnrollmentData.OrganizationUnitAddress, // Mandatory: NEW
              City = infrasecEnrollmentData.OrganizationUnitAddress, // Mandatory: NEW
              Zipcode = infrasecEnrollmentData.OrganizationUnitZipCode, // Mandatory: NEW
              Company = infrasecEnrollmentData.CompanyName // Mandatory: NEW
            },
            OperationLocation = new InfrasecNewOrUpdateEnrollmentRequest.OperationLocation
            {
              Address = infrasecEnrollmentData.OrganizationUnitAddress, // Mandatory: NEW
              City = infrasecEnrollmentData.OrganizationUnitCity, // Mandatory: NEW
              Zipcode = infrasecEnrollmentData.OrganizationUnitZipCode, // Mandatory: NEW
              Company = infrasecEnrollmentData.CompanyName // Mandatory: NEW
            },
            PcxService = new InfrasecNewOrUpdateEnrollmentRequest.PcxService
            {
              CCU = new InfrasecNewOrUpdateEnrollmentRequest.CCU
              {
                Enable = EnableCCU
              },
              Sparakvittot = new InfrasecNewOrUpdateEnrollmentRequest.Sparakvittot
              {
                // Yes/No: Enroll register for integrated Sparakvittot Digital Receipt service
                // Findity / Kivra / Sparakvittot Digital Receipt storage
                Enable = EnableSparakvittot
              },
              Swish = new InfrasecNewOrUpdateEnrollmentRequest.Swish
              {
                // Yes/No: Enroll register for integrated Swish payments
                // Swish payment notification attached to the register
                Enable = EnableSwish
              }
            }
          }
        }
      };
    }
  }
}