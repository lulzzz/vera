//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Vera.Norway
//{
//    public sealed class AuditConfigurationValidator : BaseAuditConfigurationValidator
//    {

//        public override IEnumerable<string> Validate(IOrganizationUnit organizationUnit)
//        {
//            foreach (var s in base.Validate(organizationUnit)) yield return s;
                // use config instead
//            var allowMultipleDuplicates = AuditingSettings.AllowMultipleDuplicates.GetValue(organizationUnit.ID);

//            if (allowMultipleDuplicates)
//            {
//                yield return this.MissingSetting(organizationUnit, AuditingSettings.AllowMultipleDuplicates.Key);
//            }

//            var address = organizationUnit.Address;

//            if (!organizationUnit.Type.IsCountry())
//            {
//                if (organizationUnit.Name.IsNullOrEmpty()) yield return this.MissingProperty(organizationUnit, nameof(organizationUnit.Name));
//                if (organizationUnit.BackendID.IsNullOrEmpty()) yield return this.MissingProperty(organizationUnit, nameof(organizationUnit.BackendID));

//                if (address == null)
//                {
//                    yield return this.MissingProperty(organizationUnit, nameof(organizationUnit.Address));
//                }
//                else
//                {
//                    if (address.Street.IsNullOrEmpty()) yield return this.MissingProperty(organizationUnit, nameof(address.Street));
//                    if (address.HouseNumber.IsNullOrEmpty()) yield return this.MissingProperty(organizationUnit, nameof(address.HouseNumber));
//                    if (address.ZipCode.IsNullOrEmpty()) yield return this.MissingProperty(organizationUnit, nameof(address.ZipCode));
//                    if (address.City.IsNullOrEmpty()) yield return this.MissingProperty(organizationUnit, nameof(address.City));
//                    if (address.CountryID.IsNullOrEmpty()) yield return this.MissingProperty(organizationUnit, nameof(address.CountryID));
//                }
//            }
//        }
//    }
//    public class BaseAuditConfigurationValidator : IAuditConfigurationValidator
//    {
//        public virtual IEnumerable<string> Validate(IOrganizationUnit organizationUnit)
//        {
//            var privateKey = AuditingSettings.PrivateKey.GetValue(organizationUnit.ID);

//            if (privateKey == null)
//            {
//                yield return this.MissingSetting(organizationUnit, AuditingSettings.PrivateKey.Key);
//            }

//            if (!organizationUnit.Type.IsCountry())
//            {
//                var company = organizationUnit.Company;

//                if (company == null)
//                {
//                    yield return this.MissingProperty(organizationUnit, nameof(organizationUnit.Company));
//                    yield break;
//                }

//                if (company.RegistrationNumber.IsNullOrEmpty())
//                {
//                    yield return this.MissingProperty(organizationUnit, nameof(company.RegistrationNumber));
//                }

//                if (company.VatNumber.IsNullOrEmpty())
//                {
//                    yield return this.MissingProperty(organizationUnit, nameof(company.VatNumber));
//                }

//                var address = company.VisitorsAddress;

//                if (address == null)
//                {
//                    yield return this.MissingProperty(organizationUnit, nameof(organizationUnit.Company.VisitorsAddress));
//                    yield break;
//                }

//                if (address.Street.IsNullOrEmpty())
//                {
//                    yield return $"Property {nameof(address.Street)} should be filled in for OU {organizationUnit.ID}";
//                }

//                if (address.Street.IsNullOrEmpty()) yield return this.MissingProperty(organizationUnit, nameof(address.Street));
//                if (address.HouseNumber.IsNullOrEmpty()) yield return this.MissingProperty(organizationUnit, nameof(address.HouseNumber));
//                if (address.ZipCode.IsNullOrEmpty()) yield return this.MissingProperty(organizationUnit, nameof(address.ZipCode));
//                if (address.City.IsNullOrEmpty()) yield return this.MissingProperty(organizationUnit, nameof(address.City));
//                if (address.CountryID.IsNullOrEmpty()) yield return this.MissingProperty(organizationUnit, nameof(address.CountryID));
//            }
//        }
//    }

//    public interface IAuditConfigurationValidator
//    {
//        /// <summary>
//        /// Validates the given organization unit' auditing configuration. Returns one or more strings
//        /// that indicate what could be (or is) configured incorrectly.
//        /// </summary>
//        /// <param name="organizationUnit"></param>
//        /// <returns></returns>
//        IEnumerable<string> Validate(IOrganizationUnit organizationUnit);
//    }

//    public sealed class NullAuditingConfigurationValidator : IAuditConfigurationValidator
//    {
//        public IEnumerable<string> Validate(IOrganizationUnit organizationUnit) => Enumerable.Empty<string>();
//    }

//    public static class AuditConfigurationValidatorExtensions
//    {
//        public static string MissingProperty(this IAuditConfigurationValidator validator, IOrganizationUnit organizationUnit, string property)
//        {
//            return $"Property {property} should be filled in for {organizationUnit.Name} (#{organizationUnit.ID})";
//        }

//        public static string MissingSetting(this IAuditConfigurationValidator validator, IOrganizationUnit organizationUnit, string setting)
//        {
//            return $"Setting {setting} should be set for {organizationUnit.Name} (#{organizationUnit.ID})";
//        }
//    }

//}