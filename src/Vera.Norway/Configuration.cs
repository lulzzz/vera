using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vera.Configuration;

namespace Vera.Norway
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

    }
}