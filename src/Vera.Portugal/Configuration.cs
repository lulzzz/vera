using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vera.Configuration;

namespace Vera.Portugal
{
    public class Configuration : AbstractAccountConfiguration
    {
        public override void Initialize(IDictionary<string, string> config)
        {
            string value;

            if (config.TryGetValue("PrivateKey", out value))
            {
                PrivateKey = value;
            }

            if (config.TryGetValue("PrivateKeyVersion", out value))
            {
                PrivateKeyVersion = Convert.ToInt32(value);
            }

            if (config.TryGetValue("ProductCompanyTaxId", out value))
            {
                ProductCompanyTaxId = value;
            }

            if (config.TryGetValue("SocialCapital", out value))
            {
                SocialCapital = Convert.ToDecimal(value);
            }

            if (config.TryGetValue("CertificateNumber", out value))
            {
                CertificateNumber = value;
            }

            if (config.TryGetValue("CertificateName", out value))
            {
                CertificateName = value;
            }
        }

        [Required]
        [Display(
            GroupName = "Security",
            Description = "RSA private key to use for signing the invoices"
        )]
        public string PrivateKey { get; set; }

        [Range(1, 999)]
        [Display(
            GroupName = "Security",
            Description = "Version of the private key that is being used. Must be incremented whenever the private key changes"
        )]
        public int PrivateKeyVersion { get; set; }

        [Required]
        [Display(
            GroupName = "General",
            Description = "???"
        )]
        public string ProductCompanyTaxId { get; set; }

        [Required]
        [Display(
            GroupName = "General",
            Description = "Required to be printed on all the receipts. Social capital of the company."
        )]
        public decimal SocialCapital { get; set; }

        [Required]
        [Display(
            GroupName = "Certificate",
            Description = "Required to be printed on all the receipts. Certificate name given by the authorities."
        )]
        public string CertificateName { get; set; }

        [Required]
        [Display(
            GroupName = "Certificate",
            Description = "Required to be printed on all the receipts. Certificate number given by the authorities."
        )]
        public string CertificateNumber { get; set; }
    }
}