using System;
using System.Collections.Generic;

namespace Vera.Security
{
    // "Kevin"
    // "web-service-user-1"
    public class User
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public UserType Type { get; set; }

        public Authentication Authentication { get; set; }

        public Guid CompanyId { get; set; }
    }

    public class Authentication
    {
        public string Method { get; set; }
        public byte[] Hash { get; set; }
        public byte[] Salt { get; set; }
        public int Iterations { get; set; }
    }

    public enum UserType
    {
        Normal = 1,
        Admin = 2,
        Service = 3
    }
}