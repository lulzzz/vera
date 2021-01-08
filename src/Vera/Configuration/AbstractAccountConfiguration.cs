using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Vera.Configuration
{
    public abstract class AbstractAccountConfiguration
    {
        /// <summary>
        /// Initializes the configuration from the given dictionary.
        /// </summary>
        /// <param name="config"></param>
        public abstract void Initialize(IDictionary<string, string> config);

        public IDictionary<string, object> ToDictionary()
        {
            return GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(
                    p => p.GetCustomAttribute<DisplayAttribute>()?.Name ?? p.Name,
                    p => p.GetValue(this, null)
                );
        }
    }
}