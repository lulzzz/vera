using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vera.Configuration;

namespace Vera.Portugal
{
    // TODO(kevin): extract magic strings for the keys of the fields
    public class Configuration : AbstractAccountConfiguration
    {
        public override void Initialize(IDictionary<string, string> config)
        {
            if (config.TryGetValue("PrivateKey", out var pk))
            {
                PrivateKey = pk;
            }

            if (config.TryGetValue("ProductCompanyTaxId", out var taxId))
            {
                ProductCompanyTaxId = taxId;
            }

            if (config.TryGetValue("SocialCapital", out var sc))
            {
                SocialCapital = Convert.ToDecimal(sc);
            }
        }

        [Required]
        [Display(
            Name = "PrivateKey",
            GroupName = "Security",
            Description = "RSA private key to use for signing the invoices"
        )]
        public string PrivateKey { get; set; }

        [Required]
        [Display(
            Name = "ProductCompanyTaxId",
            GroupName = "General",
            Description = "???"
        )]
        public string ProductCompanyTaxId { get; set; }

        [Required]
        [Display(
            Name = "SocialCapital",
            GroupName = "General",
            Description = "Required to be printed on all the receipts. Social capital of the company."
        )]
        public decimal SocialCapital { get; set; }
    }
}