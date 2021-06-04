using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vera.Configuration
{
    public class DefaultConfigurationValidator<T> : IConfigurationValidator where T: AbstractAccountConfiguration, new()
    {
        public ICollection<ValidationResult> Validate(IDictionary<string, string>? currentFields, IDictionary<string, string> newFields)
        {
            var config = new T();
            config.Initialize(newFields);

            var validationContext = new ValidationContext(config);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(config, validationContext, results);
            
            return results;
        }
    }
}