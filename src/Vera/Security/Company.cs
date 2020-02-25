using System;
using System.Collections.Generic;

namespace Vera.Security
{
    // "Rituals BV"
    public class Company
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public ICollection<Account> Accounts { get; set; }
        public ICollection<User> Users { get; set; }
    }
}