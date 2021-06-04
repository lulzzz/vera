using System;
using System.Collections.Generic;
using Vera.Models;
using Vera.Sweden.Models.Configs;
using Vera.Sweden.Models.Records;
using Vera.Sweden.Services;

namespace Vera.Sweden.Tests.TestHelpers
{
    public static class InfrasecModelBuilderTestHelper
    {
        public static InfrasecEnrollmentRecord BuildInfrasecEnrollmentRecord(
            string organizationUnitName = "OrganizationUnitName",
            string organizationUnitAddress = "OrganizationUnitAddress",
            string organizationUnitCity = "OrganizationUnitCity",
            string organizationUnitZipCode = "OrganizationUnitZipCode",
            string companyZipcode = "CompanyZipcode",
            string companyCity = "CompanyCity",
            string companyName = "CompanyName",
            string companyAddress = "CompanyAddress",
            string companyRegistrationNumber = "CompanyRegistrationNumber",
            string stationName = "StationName",
            string organizationUnitEmail = "OrganizationUnitEmail"
        )
        {
            return new()
            {
                OrganizationUnitName = organizationUnitName,
                OrganizationUnitAddress = organizationUnitAddress,
                OrganizationUnitCity = organizationUnitCity,
                OrganizationUnitZipCode = organizationUnitZipCode,
                CompanyZipcode = companyZipcode,
                CompanyCity = companyCity,
                CompanyName = companyName,
                CompanyAddress = companyAddress,
                CompanyRegistrationNumber = companyRegistrationNumber,
                StationName = stationName,
                OrganizationUnitEmail = organizationUnitEmail
            };
        }

        public static Account CreateAccount(Address address = null)
        {
            return new()
            {
                Address = address ?? new Address
                {
                    City = "City",
                    PostalCode = "PostalCode",
                    Street = "Street",
                    Number = "Number"
                }
            };
        }
        
        public static Supplier CreateSupplier(Guid id = new(),
            string systemId = "SystemId", 
            string name = "Name",
            string registrationNumber = "RegistrationNumber",
            string taxRegistrationNumber = "TaxRegistrationNumber",
            Address address = null,
            Guid accountId = new(),
            string timeZone = "TimeZone",
            string emailAddress = "EmailAddress")
        {
            return new()
            {
               Id = id,
               SystemId = systemId,
               Address = address,
               Name = name,
               AccountId = accountId,
               EmailAddress = emailAddress,
               RegistrationNumber = registrationNumber,
               TimeZone = timeZone,
               TaxRegistrationNumber = taxRegistrationNumber
            };
        }

        public static Register CreateRegister(Guid id = new(),
            string name = "Name",
            string fiscalSystemId = null,
            Guid supplierId = new())
        {
            return new()
            {
                Id = id,
                Name = name,
                Data = fiscalSystemId == null ? new Dictionary<string, string>()
                    : new Dictionary<string, string>
                {
                    {"FiscalSystemId", fiscalSystemId}
                },
                SupplierId = supplierId
            };
        }

        public static SwedenConfigs BuildSwedenConfigs()
        {
            return new()
            {
                MandatoryEnrollmentSettingFields = BuildMandatoryEnrollmentSettingFields(),
                InfrasecEnrollmentApiUrl = "InfrasecEnrollmentApiUrl",
                InfrasecEnrollmentCertPfx = new byte[1] {0x01},
                InfrasecReceiptApiUrl = "InfrasecReceiptApiUrl",
                InfrasecReceiptCertPfx = new byte[1] {0x02},
                InfrasecEnrollmentCertPfxKey = "InfrasecEnrollmentCertPfxKey",
                InfrasecReceiptCertPfxKey = "InfrasecReceiptCertPfxKey",
                InfrasecEnrollmentCertServerTrustPem = new byte[1] {0x03},
                InfrasecReceiptCertServerTrustPem = new byte[1] {0x04}
            };
        }

        public static MandatoryEnrollmentSettingFields BuildMandatoryEnrollmentSettingFields(
            string shopNumber = "1234", string tenantName = "tenantName", string tenantCode = "12345",
            string posAuthorityCode = "POS Authority Code")
        {
            return new()
            {
                ShopNumber = shopNumber,
                TenantName = tenantName,
                TenantCode = tenantCode,
                InfrasecApiPosAuthorityCode = posAuthorityCode
            };
        }
    }
}