using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vera.Configuration;

namespace Vera.Poland
{
    public class Configuration : AbstractAccountConfiguration
    {
        public override void Initialize(IDictionary<string, string> config)
        {
            if (config.TryGetValue(nameof(Logo), out var value))
            {
                Logo = Guid.Parse(value);
            }

            if (config.TryGetValue(nameof(WelcomeMessage), out value))
                WelcomeMessage = value.Split(',');
        }
     
        [Display(
            GroupName = "Configuration",
            Description = "Id of the printer logo blob")]
        public Guid? Logo { get; set; }

        [Display(GroupName = "Configuration", Description = "Welcome message shown on the printer")]
        public string[] WelcomeMessage { get; set; } = Array.Empty<string>();
    }
}