using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vera.Configuration;

namespace Vera.Germany
{
    public class ConfigurationValidator : IConfigurationValidator
    {
        public ICollection<ValidationResult> Validate(
            IDictionary<string, string> currentFields, 
            IDictionary<string, string> newFields)
        {
            return new List<ValidationResult>();
        }
    }
}
