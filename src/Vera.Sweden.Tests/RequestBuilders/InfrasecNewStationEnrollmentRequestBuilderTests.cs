using FakeItEasy;
using Vera.Sweden.Models.Configs;
using Vera.Sweden.Models.Constants;
using Vera.Sweden.Models.Enums;
using Vera.Sweden.Models.Records;
using Vera.Sweden.RequestBuilders;
using Vera.Sweden.RequestBuilders.Contracts;
using Vera.Sweden.Tests.TestHelpers;
using Vera.Sweden.Validators.Contracts;
using Xunit;

namespace Vera.Sweden.Tests.RequestBuilders
{
  public class InfrasecNewStationEnrollmentRequestBuilderTests
  {
    private readonly IInfrasecEnrollmentRequestValidator _infrasecEnrollmentValidator;
    private readonly IInfrasecNewStationEnrollmentRequestBuilder _requestBuilder;

    public InfrasecNewStationEnrollmentRequestBuilderTests()
    {
      _infrasecEnrollmentValidator = A.Fake<IInfrasecEnrollmentRequestValidator>();
      _requestBuilder = new InfrasecNewStationEnrollmentRequestBuilder(_infrasecEnrollmentValidator);
    }

    [Fact]
    public void Will_Assign_Properties_As_Expected()
    {
      const string shopNumber = "15";
      const string tenantCode = "33";
      const string tenantName = "ChainName-Unit-Test";
      const int currentStationNumber = 44;

      A.CallTo(() => _infrasecEnrollmentValidator.SanitizeStationNumber(A<int>.Ignored)).Returns(currentStationNumber.ToString());

      var expectedRequest = InfrasecModelBuilderTestHelper.BuildInfrasecEnrollmentRecord();
      var expectedSettingFields = InfrasecModelBuilderTestHelper.BuildMandatoryEnrollmentSettingFields(shopNumber, tenantName, tenantCode);

      var request = _requestBuilder.BuildRequest(InfrasecModelBuilderTestHelper.BuildMandatoryEnrollmentSettingFields(shopNumber, tenantName, tenantCode),
        InfrasecModelBuilderTestHelper.BuildInfrasecEnrollmentRecord(), currentStationNumber);

      Assert.Equal(36, request.IdmRequest.RequestID.Length); // GUID length
      Assert.Equal(SwedenAuditingConstants.InfrasecApplicationID, request.IdmRequest.ApplicationID);
      Assert.Equal(AvailableInfrasecRequestActions.NEW.ToString(), request.IdmRequest.EnrollData.Action);
      Assert.Equal(SwedenAuditingConstants.InfrasecApiPartnerCode, request.IdmRequest.EnrollData.PartnerAuthority.PartnerCode);
      Assert.Equal(SwedenAuditingConstants.InfrasecApiPartnerName, request.IdmRequest.EnrollData.PartnerAuthority.PartnerName);
      Assert.Equal(expectedSettingFields.InfrasecApiPosAuthorityCode, request.IdmRequest.EnrollData.PartnerAuthority.POSAuthorityCode);

      Assert.Equal(tenantName, request.IdmRequest.EnrollData.OrganizationChain.ChainName);
      Assert.Equal(expectedSettingFields.TenantCode, request.IdmRequest.EnrollData.OrganizationChain.ChainCode); // normally sanitized to 00033 by the validator

      Assert.Equal(expectedSettingFields.ShopNumber, request.IdmRequest.EnrollData.StoreInfo.StoreID); // normally sanitized to 0015 by the validator
      Assert.Equal(expectedRequest.OrganizationUnitName, request.IdmRequest.EnrollData.StoreInfo.StoreName);
      Assert.Equal(expectedRequest.OrganizationUnitAddress, request.IdmRequest.EnrollData.StoreInfo.Address);
      Assert.Equal(expectedRequest.OrganizationUnitCity, request.IdmRequest.EnrollData.StoreInfo.City);
      Assert.Equal(expectedRequest.OrganizationUnitZipCode, request.IdmRequest.EnrollData.StoreInfo.Zipcode);
      Assert.Equal(expectedRequest.OrganizationUnitEmail, request.IdmRequest.EnrollData.StoreInfo.Email);

      Assert.Equal(expectedRequest.CompanyZipcode, request.IdmRequest.EnrollData.CompanyInfo.Zipcode);
      Assert.Equal(expectedRequest.CompanyCity, request.IdmRequest.EnrollData.CompanyInfo.City);
      Assert.Equal(expectedRequest.CompanyAddress, request.IdmRequest.EnrollData.CompanyInfo.Address);
      Assert.Equal(expectedRequest.CompanyName, request.IdmRequest.EnrollData.CompanyInfo.Company);
      Assert.Equal(expectedRequest.CompanyRegistrationNumber, request.IdmRequest.EnrollData.CompanyInfo.OrganizationNumber);

      Assert.Equal(expectedRequest.StationName, request.IdmRequest.EnrollData.RegisterInfo.RegisterMake);
      Assert.Equal(expectedRequest.StationName, request.IdmRequest.EnrollData.RegisterInfo.RegisterModel);
      Assert.Equal(expectedRequest.OrganizationUnitAddress, request.IdmRequest.EnrollData.RegisterInfo.Address);
      Assert.Equal(expectedRequest.OrganizationUnitZipCode, request.IdmRequest.EnrollData.RegisterInfo.Zipcode);
      Assert.Equal(expectedRequest.OrganizationUnitCity, request.IdmRequest.EnrollData.RegisterInfo.City);
      Assert.Equal(currentStationNumber.ToString(), request.IdmRequest.EnrollData.RegisterInfo.CounterNumber); // normally sanitized to 044 by the validator

      Assert.Equal(expectedRequest.OrganizationUnitAddress, request.IdmRequest.EnrollData.JournalLocation.Address);
      Assert.Equal(expectedRequest.OrganizationUnitAddress, request.IdmRequest.EnrollData.JournalLocation.City);
      Assert.Equal(expectedRequest.OrganizationUnitZipCode, request.IdmRequest.EnrollData.JournalLocation.Zipcode);
      Assert.Equal(expectedRequest.CompanyName, request.IdmRequest.EnrollData.JournalLocation.Company);

      Assert.Equal("Yes", request.IdmRequest.EnrollData.PcxService.CCU.Enable);
      Assert.Equal("No", request.IdmRequest.EnrollData.PcxService.Sparakvittot.Enable);
      Assert.Equal("No", request.IdmRequest.EnrollData.PcxService.Swish.Enable);

      A.CallTo(() => _infrasecEnrollmentValidator.ValidateCompanyRegistrationNumber(expectedRequest.CompanyRegistrationNumber)).MustHaveHappenedOnceExactly();
      A.CallTo(() => _infrasecEnrollmentValidator.ValidateSwedenAuditingConstants()).MustHaveHappenedOnceExactly();
      A.CallTo(() => _infrasecEnrollmentValidator.ValidateInfrasecEnrollmentData(A<InfrasecEnrollmentRecord>.Ignored)).MustHaveHappenedOnceExactly();
      A.CallTo(() => _infrasecEnrollmentValidator.ValidateCurrentStationNumber(A<int>.Ignored)).MustHaveHappenedOnceExactly();
      A.CallTo(() => _infrasecEnrollmentValidator.SanitizeSettingFields(A<MandatoryEnrollmentSettingFields>.Ignored)).MustHaveHappenedOnceExactly();
      A.CallTo(() => _infrasecEnrollmentValidator.ValidateSanitizedFields(A<MandatoryEnrollmentSettingFields>.Ignored)).MustHaveHappenedOnceExactly();
      A.CallTo(() => _infrasecEnrollmentValidator.SanitizeStationNumber(currentStationNumber)).MustHaveHappenedOnceExactly();
    }
  }
}