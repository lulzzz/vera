using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vera.Configuration;

namespace Vera.Poland
{
    public class ConfigurationValidator : IConfigurationValidator
    {
        public ICollection<ValidationResult> Validate(
            IDictionary<string, string>? currentFields,
            IDictionary<string, string> newFields
        )
        {
            var config = new Configuration();
            config.Initialize(newFields);

            var validationContext = new ValidationContext(config);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(config, validationContext, results);

            if (currentFields == null) return results;

            var currentConfig = new Configuration();
            currentConfig.Initialize(currentFields);


            return results;
        }
    }
}
