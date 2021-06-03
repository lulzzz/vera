using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vera.Configuration;

namespace Vera.Germany
{
    public class Configuration : AbstractAccountConfiguration
    {
        public override void Initialize(IDictionary<string, string> config)
        {
            string value;

            if (config.TryGetValue("ApiKey", out value))
            {
                ApiKey = value;
            }

            if (config.TryGetValue("ApiSecret", out value))
            {
                ApiSecret = value;
            }

            if (config.TryGetValue("BaseUrl", out value))
            {
                BaseUrl = value;
            }
        }

        [Required]
        [Display(
            GroupName = "Security",
            Description = "Api key for authentication to fiskaly service"
        )]
        public string ApiKey { get; private set; }

        [Required]
        [Display(
            GroupName = "Security",
            Description = "Api secret for authentication to fiskaly service"
        )]
        public string ApiSecret { get; private set; }

        [Required]
        [Display(
            GroupName = "Security",
            Description = "Base url to fiskaly service"
        )]
        public string BaseUrl { get; private set; }
    }
}
