using System.ComponentModel.DataAnnotations;

namespace Vera.Sweden.Models.Configs
{
    public class MandatoryEnrollmentSettingFields
    {
        [Display(
            GroupName = "InfrasecRegisterIdentity",
            Description = "See Vera.Sweden/README.md - Use 01 for testing"
        )]
        public string InfrasecApiPosAuthorityCode { get; set; }

        [Display(
            GroupName = "InfrasecRegisterIdentity",
            Description = "See Vera.Sweden/README.md - Maximum 5 characters, if shorter, " +
                          "it will be prefixed with 0. E.g. 243 will become 00243."
        )]
        public string TenantCode { get; set; }

        [Display(
            GroupName = "InfrasecRegisterIdentity",
            Description = "See Vera.Sweden/README.md - Infrasec expects this, but will not affect" +
                          "the value of the Register Unique Identity"
        )]
        public string TenantName { get; set; }

        [Display(
            GroupName = "InfrasecRegisterIdentity",
            Description = "See Vera.Sweden/README.md - Maximum 3 characters, if shorter, " +
                          "it will be prefixed with 0. E.g. 11 will become 011."
        )]
        public string ShopNumber { get; set; }
    }

    public class SwedenConfigs
    {
        public MandatoryEnrollmentSettingFields MandatoryEnrollmentSettingFields { get; init; }

        // Infrasec EnrollmentAPI
        [Required]
        [Display(
            GroupName = "InfrasecEnrollment",
            Description = "Infrasec Enrollment API Url"
        )]
        public string InfrasecEnrollmentApiUrl { get; set; }

        [Required]
        [Display(
            GroupName = "InfrasecEnrollment",
            Description = "Infrasec Enrollment API Certificate (.pfx)"
        )]
        public byte[] InfrasecEnrollmentCertPfx { get; set; }

        [Required]
        [Display(
            GroupName = "InfrasecEnrollment",
            Description = "Infrasec Enrollment API Certificate Key (.pfx)"
        )]
        public string InfrasecEnrollmentCertPfxKey { get; set; }

        [Required]
        [Display(
            GroupName = "InfrasecEnrollment",
            Description = "File provided by Infrasec to validate the certificate (.pem)"
        )]
        public byte[] InfrasecEnrollmentCertServerTrustPem { get; set; }

        // Infrasec ReceiptAPI
        [Display(
            GroupName = "InfrasecReceipt",
            Description = "Infrasec Receipt API Url"
        )]
        public string InfrasecReceiptApiUrl { get; set; }

        [Display(
            GroupName = "InfrasecReceipt",
            Description = "Infrasec Receipt API Certificate (.pfx)"
        )]
        public byte[] InfrasecReceiptCertPfx { get; set; }

        [Display(
            GroupName = "InfrasecReceipt",
            Description = "Infrasec Receipt API Certificate Key (.pfx)"
        )]
        public string InfrasecReceiptCertPfxKey { get; set; }

        [Display(
            GroupName = "InfrasecReceipt",
            Description = "File provided by Infrasec to validate the certificate (.pem)"
        )]
        public byte[] InfrasecReceiptCertServerTrustPem { get; set; }
    }
}