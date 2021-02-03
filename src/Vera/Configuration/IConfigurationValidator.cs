using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vera.Configuration
{
    public interface IConfigurationValidator
    {
        ICollection<ValidationResult> Validate(
            IDictionary<string, string>? currentFields,
            IDictionary<string, string> newFields
        );
    }
}