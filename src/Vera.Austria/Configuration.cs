using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vera.Configuration;

namespace Vera.Austria
{
    public class Configuration : AbstractAccountConfiguration
    {
        public override void Initialize(IDictionary<string, string> config)
        {
            string value;

            if (config.TryGetValue("AESKey", out value))
            {
                AESKey = value;
            }

            if (config.TryGetValue("DEPHost", out value))
            {
                DEPHost = value;
            }

            if (config.TryGetValue("DEPUsername", out value))
            {
                DEPUsername = value;
            }
            
            if (config.TryGetValue("DEPPassword", out value))
            {
                DEPPassword = value;
            }

            if (config.TryGetValue("ATrustHost", out value))
            {
                ATrustHost = value;
            }

            if (config.TryGetValue("ATrustUsername", out value))
            {
                ATrustUsername = value;
            }

            if (config.TryGetValue("ATrustPassword", out value))
            {
                ATrustPassword = value;
            }
        }
        
        [Required]
        [Display(
            GroupName = "General",
            Description = "Hexidecimal string of the AES key that is used to encrypt the transaction counter value."
        )]
        public string AESKey { get; set; }

        [Required]
        public string DEPHost { get; set; }

        [Required]
        public string DEPUsername { get; set; }

        [Required]
        public string DEPPassword { get; set; }

        [Required]
        public string ATrustHost { get; set; }

        [Required]
        public string ATrustUsername { get; set; }

        [Required]
        public string ATrustPassword { get; set; }
    }
}