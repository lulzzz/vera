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
        }

        [Required]
        [Display(
            GroupName = "Security",
            Description = "RSA private key to use for signing the invoices"
        )]
        public string PrivateKey { get; set; }

    }
}