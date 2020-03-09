using System;
using System.Collections.Generic;

namespace Vera
{
    // "Rituals Norway"
    public class Account
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Certification { get; set; }

        public IDictionary<string, string> Configuration { get; set; }
    }
}