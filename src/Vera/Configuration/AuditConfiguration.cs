using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Vera.Configuration
{
    public abstract class AbstractAuditConfiguration
    {
        // TODO(kevin): add generic T to IAuditConfiguration, let this method return validation result?
        public abstract void Initialize(IDictionary<string, object> config);

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