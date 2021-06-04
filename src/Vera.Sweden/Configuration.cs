using System.Collections.Generic;
using System.Text;
using Vera.Configuration;
using Vera.Sweden.Models.Configs;

namespace Vera.Sweden
{
    public class Configuration : AbstractAccountConfiguration
    {
        public readonly SwedenConfigs SwedenConfigs = new();

        public override void Initialize(IDictionary<string, string> config)
        {
            SetInfrasecEnrollmentConfigs(config);
            SetInfrasecReceiptConfigs(config);
            SetInfrasecRegisterIdentityConfigs(config);
        }

        private void SetInfrasecRegisterIdentityConfigs(IDictionary<string, string> config)
        {
            string value;
            if (config.TryGetValue(nameof(SwedenConfigs.MandatoryEnrollmentSettingFields.InfrasecApiPosAuthorityCode), out value))
            {
                SwedenConfigs.MandatoryEnrollmentSettingFields.InfrasecApiPosAuthorityCode = value;
            }

            if (config.TryGetValue(nameof(SwedenConfigs.MandatoryEnrollmentSettingFields.TenantCode), out value))
            {
                SwedenConfigs.MandatoryEnrollmentSettingFields.TenantCode = value;
            }

            if (config.TryGetValue(nameof(SwedenConfigs.MandatoryEnrollmentSettingFields.TenantName), out value))
            {
                SwedenConfigs.MandatoryEnrollmentSettingFields.TenantName = value;
            }

            if (config.TryGetValue(nameof(SwedenConfigs.MandatoryEnrollmentSettingFields.ShopNumber), out value))
            {
                SwedenConfigs.MandatoryEnrollmentSettingFields.ShopNumber = value;
            }
        }

        private void SetInfrasecReceiptConfigs(IDictionary<string, string> config)
        {
            string value;
            if (config.TryGetValue(nameof(SwedenConfigs.InfrasecReceiptApiUrl), out value))
            {
                SwedenConfigs.InfrasecReceiptApiUrl = value;
            }

            if (config.TryGetValue(nameof(SwedenConfigs.InfrasecReceiptCertPfx), out value))
            {
                SwedenConfigs.InfrasecReceiptCertPfx = Encoding.ASCII.GetBytes(value);
            }

            if (config.TryGetValue(nameof(SwedenConfigs.InfrasecReceiptCertPfxKey), out value))
            {
                SwedenConfigs.InfrasecReceiptCertPfxKey = value;
            }

            if (config.TryGetValue(nameof(SwedenConfigs.InfrasecReceiptCertServerTrustPem), out value))
            {
                SwedenConfigs.InfrasecReceiptCertServerTrustPem = Encoding.ASCII.GetBytes(value);
            }
        }

        private void SetInfrasecEnrollmentConfigs(IDictionary<string, string> config)
        {
            string value;
            if (config.TryGetValue(nameof(SwedenConfigs.InfrasecEnrollmentApiUrl), out value))
            {
                SwedenConfigs.InfrasecEnrollmentApiUrl = value;
            }

            if (config.TryGetValue(nameof(SwedenConfigs.InfrasecEnrollmentCertPfx), out value))
            {
                SwedenConfigs.InfrasecEnrollmentCertPfx = Encoding.ASCII.GetBytes(value);
            }

            if (config.TryGetValue(nameof(SwedenConfigs.InfrasecEnrollmentCertPfxKey), out value))
            {
                SwedenConfigs.InfrasecEnrollmentCertPfxKey = value;
            }

            if (config.TryGetValue(nameof(SwedenConfigs.InfrasecEnrollmentCertServerTrustPem), out value))
            {
                SwedenConfigs.InfrasecEnrollmentCertServerTrustPem = Encoding.ASCII.GetBytes(value);
            }
        }
    }
}