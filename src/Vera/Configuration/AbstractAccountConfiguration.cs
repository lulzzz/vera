using System.Collections.Generic;

namespace Vera.Configuration
{
    public abstract class AbstractAccountConfiguration
    {
        /// <summary>
        /// Initializes the configuration from the given dictionary.
        /// </summary>
        /// <param name="config"></param>
        public abstract void Initialize(IDictionary<string, string> config);
    }
}