using System;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Vera.Models;
using Vera.Sweden.Models.Configs;
using Vera.Sweden.Models.Exceptions;
using Vera.Sweden.Models.Records;
using Vera.Sweden.Tests.TestHelpers;
using Vera.Sweden.Validators;
using Vera.Sweden.Validators.Contracts;
using Xunit;

namespace Vera.Sweden.Tests.Validators
{
  public class InfrasecEnrollmentRequestValidatorTests
  {
    private readonly IInfrasecEnrollmentRequestValidator _infrasecEnrollmentValidator;

    public InfrasecEnrollmentRequestValidatorTests()
    {
      _infrasecEnrollmentValidator = new InfrasecEnrollmentRequestValidator();
    }

    // ValidateSettingFields
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void When_ShopNumber_Is_Null_Or_Whitespace_Will_Throw(string input)
    {
      var exception = Assert.Throws<ArgumentNullException>(() =>
      {
        _infrasecEnrollmentValidator.ValidateSettingFields(InfrasecModelBuilderTestHelper.BuildMandatoryEnrollmentSettingFields(shopNumber: input));
      });

      Assert.Equal(nameof(MandatoryEnrollmentSettingFields.ShopNumber), exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void When_TenantName_Is_Null_Or_Whitespace_Will_Throw(string input)
    {
      var exception = Assert.Throws<ArgumentNullException>(() =>
      {
        _infrasecEnrollmentValidator.ValidateSettingFields(InfrasecModelBuilderTestHelper.BuildMandatoryEnrollmentSettingFields(tenantName: input));
      });

      Assert.Equal(nameof(MandatoryEnrollmentSettingFields.TenantName), exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void When_TenantCode_Is_Null_Or_Whitespace_Will_Throw(string input)
    {
      var exception = Assert.Throws<ArgumentNullException>(() =>
      {
        _infrasecEnrollmentValidator.ValidateSettingFields(InfrasecModelBuilderTestHelper.BuildMandatoryEnrollmentSettingFields(tenantCode: input));
      });

      Assert.Equal(nameof(MandatoryEnrollmentSettingFields.TenantCode), exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void When_POSAuthorityCode_Is_Null_Or_Whitespace_Will_Throw(string input)
    {
      var exception = Assert.Throws<ArgumentNullException>(() =>
      {
        _infrasecEnrollmentValidator.ValidateSettingFields(InfrasecModelBuilderTestHelper.BuildMandatoryEnrollmentSettingFields(posAuthorityCode: input));
      });

      Assert.Equal(nameof(MandatoryEnrollmentSettingFields.InfrasecApiPosAuthorityCode), exception.ParamName);
    }

    // SanitizeSettingFields
    [Fact]
    public void Will_Sanitize_mandatory_enrollment_Setting_Fields()
    {
      var result = _infrasecEnrollmentValidator.SanitizeSettingFields(new MandatoryEnrollmentSettingFields
      {
        ShopNumber = "11",
        TenantCode = "22",
        InfrasecApiPosAuthorityCode = "Unmodified POS Authority Code"
      });

      Assert.Equal("0011", result.ShopNumber);
      Assert.Equal("00022", result.TenantCode);
      Assert.Equal("Unmodified POS Authority Code", result.InfrasecApiPosAuthorityCode);
    }

    // SanitizeStationNumber
    [Fact]
    public void Will_Sanitize_Station_Number()
    {
      var result = _infrasecEnrollmentValidator.SanitizeStationNumber(3);

      Assert.Equal("003", result);
    }

    // ValidateSanitizedFields
    [Theory]
    [InlineData("123")]
    [InlineData("12345")]
    public void Will_Validate_Sanitized_ShopNumber(string shopNumber)
    {
      var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
      {
        _infrasecEnrollmentValidator.ValidateSanitizedFields(InfrasecModelBuilderTestHelper.BuildMandatoryEnrollmentSettingFields(shopNumber: shopNumber));
      });

      Assert.Equal(nameof(SwedenConfigs.MandatoryEnrollmentSettingFields.ShopNumber),exception.ParamName);
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("123456")]
    public void Will_Validate_Sanitized_TenantCode(string tenantCode)
    {
      var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
      {
        _infrasecEnrollmentValidator.ValidateSanitizedFields(InfrasecModelBuilderTestHelper.BuildMandatoryEnrollmentSettingFields(tenantCode: tenantCode));
      });

      Assert.Equal(nameof(SwedenConfigs.MandatoryEnrollmentSettingFields.TenantCode),exception.ParamName);
    }

    // ValidateCurrentStationNumber
    [Theory]
    [InlineData(-1)]
    [InlineData(1000)]
    public void Will_Validate_Sanitized_CurrentStationNumber(int currentStationNumber)
    {
      var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
      {
        _infrasecEnrollmentValidator.ValidateCurrentStationNumber(currentStationNumber);
      });

      Assert.Equal(nameof(currentStationNumber),exception.ParamName);
    }

    // Validate RegisterID
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Will_Validate_InfrasecRegisterID(string registerID)
    {
      Assert.Throws<InfrasecRegisterIdNullOrWhitespace>(() =>
      {
        _infrasecEnrollmentValidator.ValidateInfrasecRegisterID(registerID);
      });
    }

    // ValidateCompanyRegistrationNumber
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("11111111110")]
    [InlineData("0001")]
    public void Will_Validate_CompanyRegistrationNumber(string registrationNumber)
    {
      var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
      {
        _infrasecEnrollmentValidator.ValidateCompanyRegistrationNumber(registrationNumber);
      });

      Assert.Equal(nameof(Supplier.RegistrationNumber),exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void When_StoreName_Is_Null_Or_Empty_Will_Throw(string prop)
    {
      var exception = Assert.Throws<ArgumentNullException>(() =>
      {
        _infrasecEnrollmentValidator.ValidateInfrasecEnrollmentData(InfrasecModelBuilderTestHelper.BuildInfrasecEnrollmentRecord(organizationUnitName: prop));
      });

      Assert.Equal(nameof(InfrasecEnrollmentRecord.OrganizationUnitName), exception.ParamName);
    }

    [Fact]
    public void When_Input_String_Is_Null_WillThrow()
    {
      Assert.Throws<ArgumentNullException>(() =>
      {
        InfrasecEnrollmentRequestValidator.AddStringPrefix(null, 'c', 5);
      });
    }

    [Theory]
    [InlineData("test")]
    [InlineData("t")]
    public void When_No_Padding_Needs_To_Be_Performed_Will_Return_Original_String(string inputString)
    {
      var result = InfrasecEnrollmentRequestValidator.AddStringPrefix(inputString, '0', 1);
      Assert.Equal(inputString, result);
    }

    [Theory]
    [InlineData("test", '0', 10, "000000test")]
    [InlineData("t", '*', 3, "**t")]
    [InlineData("t", '*', 0, "t")]
    public void Will_Format_As_Expected(string inputString, char padWith, int expectedLength, string expectedString)
    {
      var result = InfrasecEnrollmentRequestValidator.AddStringPrefix(inputString, padWith, expectedLength);
      Assert.Equal(expectedString, result);
    }
  }
}