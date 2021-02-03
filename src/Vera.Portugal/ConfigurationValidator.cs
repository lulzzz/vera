using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vera.Configuration;

namespace Vera.Portugal
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

            if (!HasPrivateKeyBeenModifiedCorrectly(currentConfig, config))
            {
                results.Add(new ValidationResult("private key changed, version should be incremented accordingly"));
            }

            return results;
        }

        private static bool HasPrivateKeyBeenModifiedCorrectly(Configuration current, Configuration config)
        {
            // Verify that when the private key is modified that the version is incremented
            return current.PrivateKey != config.PrivateKey && current.PrivateKeyVersion < config.PrivateKeyVersion;
        }
    }
}