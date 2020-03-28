using System;
using System.Collections.Generic;
using Vera.Configuration;
using Vera.Models;

namespace Vera
{
    // "Rituals Norway"
    public class Account
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string RegistrationNumber { get; set; }
        public Address Address { get; set; }
        
        public string Certification { get; set; }

        public IDictionary<string, object> Configuration { get; set; }
        public string Currency { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string TaxRegistrationNumber { get; set; }

        // TODO(kevin): move T to Configuration property?
        public T GetConfiguration<T>() where T : AbstractAuditConfiguration, new()
        {
            if (Configuration == null)
            {
                return new T();
            }

            var config = new T();
            config.Initialize(Configuration);

            return config;
        }

        public void SetConfiguration(AbstractAuditConfiguration config)
        {
            Configuration = config?.ToDictionary() ?? new Dictionary<string, object>();
        }
    }
}