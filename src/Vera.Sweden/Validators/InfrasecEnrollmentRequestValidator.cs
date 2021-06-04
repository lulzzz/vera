using System;
using Vera.Extensions;
using Vera.Models;
using Vera.Sweden.Models.Configs;
using Vera.Sweden.Models.Constants;
using Vera.Sweden.Models.Exceptions;
using Vera.Sweden.Models.Records;
using Vera.Sweden.Validators.Contracts;

namespace Vera.Sweden.Validators
{
    public class InfrasecEnrollmentRequestValidator : IInfrasecEnrollmentRequestValidator
    {
        private const char InfrasecPaddingCharacter = '0';

        // Counter (Station) Number: Max 3 chars
        private const int InfrasecExpectedRegisterNumberLength = 3;

        // Shop Number (StoreID): Max 4 chars
        private const int InfrasecExpectedShopNumberLength = 4;

        // Tenant (Chain Code): Max 5 chars
        private const int InfrasecExpectedTenantCodeLength = 5;

        // Swedish companies must have exactly 10 characters
        private const int InfrasecExpectedOrganizationNumberLength = 10;
        
        public void ValidateCompanyRegistrationNumber(string registrationNumber)
        {
            if (registrationNumber.IsNullOrWhiteSpace() ||
                registrationNumber.Length != InfrasecExpectedOrganizationNumberLength)
            {
                throw new ArgumentOutOfRangeException(nameof(Supplier.RegistrationNumber),
                    $"CompanyRegistrationNumberNotWithinRange",
                    $"The Infrasec Organization Number (taken from the Company RegistrationNumber) must be exactly " +
                    $"{InfrasecExpectedOrganizationNumberLength}, but it was: [{registrationNumber?.Length}] - {registrationNumber}");
            }
        }

        public void ValidateSettingFields(MandatoryEnrollmentSettingFields fieldsFromSettings)
        {
            if (fieldsFromSettings.ShopNumber.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(fieldsFromSettings.ShopNumber),
                    $"{fieldsFromSettings.ShopNumber} cannot be null or whitespace");
            }

            if (fieldsFromSettings.TenantName.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(fieldsFromSettings.TenantName),
                    $"{fieldsFromSettings.TenantName} cannot be null or whitespace");
            }

            if (fieldsFromSettings.TenantCode.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(fieldsFromSettings.TenantCode),
                    $"{fieldsFromSettings.TenantCode} cannot be null or whitespace");
            }

            if (fieldsFromSettings.InfrasecApiPosAuthorityCode.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(fieldsFromSettings.InfrasecApiPosAuthorityCode),
                    $"{fieldsFromSettings.InfrasecApiPosAuthorityCode} cannot be null or whitespace");
            }
        }

        public MandatoryEnrollmentSettingFields SanitizeSettingFields(
            MandatoryEnrollmentSettingFields fieldsFromSettings)
        {
            fieldsFromSettings.ShopNumber =
                AddStringPrefix(fieldsFromSettings.ShopNumber, InfrasecPaddingCharacter,
                    InfrasecExpectedShopNumberLength);
            fieldsFromSettings.TenantCode =
                AddStringPrefix(fieldsFromSettings.TenantCode, InfrasecPaddingCharacter,
                    InfrasecExpectedTenantCodeLength);

            return fieldsFromSettings;
        }

        public string SanitizeStationNumber(int currentStationNumber)
        {
            return AddStringPrefix(currentStationNumber.ToString(),
                InfrasecPaddingCharacter,
                InfrasecExpectedRegisterNumberLength);
        }

        public void ValidateSanitizedFields(MandatoryEnrollmentSettingFields fieldsFromSettings)
        {
            if (fieldsFromSettings.ShopNumber.Length != InfrasecExpectedShopNumberLength)
            {
                throw new ArgumentOutOfRangeException(nameof(fieldsFromSettings.ShopNumber),
                    $"ShopID exceeds maximum length of {InfrasecExpectedShopNumberLength} characters, but it was: [{fieldsFromSettings.ShopNumber.Length}] - {fieldsFromSettings.ShopNumber}");
            }

            if (fieldsFromSettings.TenantCode.Length != InfrasecExpectedTenantCodeLength)
            {
                throw new ArgumentOutOfRangeException(nameof(fieldsFromSettings.TenantCode),
                    $"TenantCode exceeds maximum length of {InfrasecExpectedTenantCodeLength} characters, but it was: [{fieldsFromSettings.TenantCode.Length}] - {fieldsFromSettings.TenantCode}");
            }
        }

        public void ValidateCurrentStationNumber(int currentStationNumber)
        {
            if (currentStationNumber is > 999 or < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentStationNumber), $"CurrentStationNumber must be between 999 and 0 characters, but it was: {currentStationNumber}");
            }
        }

        public void ValidateInfrasecRegisterID(string infrasecRegisterID)
        {
            if (infrasecRegisterID.IsNullOrWhiteSpace())
            {
                throw new InfrasecRegisterIdNullOrWhitespace("Station was expected to have a RegisterID assigned, but it was null or whitespace");
            }
        }

        // If it's worth doing, it's worth overdoing
        public void ValidateSwedenAuditingConstants()
        {
            if (SwedenAuditingConstants.InfrasecApplicationID.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(SwedenAuditingConstants.InfrasecApplicationID),
                    $"{SwedenAuditingConstants.InfrasecApplicationID} cannot be null or whitespace");
            }

            if (SwedenAuditingConstants.InfrasecApiPartnerCode.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(SwedenAuditingConstants.InfrasecApiPartnerCode),
                    $"{SwedenAuditingConstants.InfrasecApiPartnerCode} cannot be null or whitespace");
            }

            if (SwedenAuditingConstants.InfrasecApiPartnerName.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(SwedenAuditingConstants.InfrasecApiPartnerName),
                    $"{SwedenAuditingConstants.InfrasecApiPartnerName} cannot be null or whitespace");
            }

            if (SwedenAuditingConstants.InfrasecApiPartnerCode.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(SwedenAuditingConstants.InfrasecApiPartnerCode),
                    $"{SwedenAuditingConstants.InfrasecApiPartnerCode} cannot be null or whitespace");
            }
        }

        public void ValidateInfrasecEnrollmentData(InfrasecEnrollmentRecord infrasecNewOrUpdateEnrollmentData)
        {
            var inputType = infrasecNewOrUpdateEnrollmentData.GetType();

            foreach (var propertyInfo in inputType.GetProperties())
            {
                if (propertyInfo.PropertyType != typeof(string))
                {
                    throw new InvalidOperationException($"Current validation only handles strings!");
                }

                var propValue = (string) propertyInfo.GetValue(infrasecNewOrUpdateEnrollmentData);
                if (propValue.IsNullOrWhiteSpace())
                {
                    throw new ArgumentNullException(propertyInfo.Name,
                        $"{propertyInfo.Name} cannot be null or whitespace");
                }
            }
        }

        public static string AddStringPrefix(string model, char padWith, int expectedLength)
        {
            if (model == null) // ignore empty / whitespace for now
            {
                throw new ArgumentNullException(model, "Input string cannot be null");
            }

            if (expectedLength > model.Length)
            {
                return model.PadLeft(expectedLength, padWith);
            }

            return model;
        }
    }
}