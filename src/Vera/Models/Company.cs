using System;
using System.Collections.Generic;

namespace Vera.Models
{
    // "Rituals BV"
    public class Company
    {
        public Company()
        {
        }

        public Company(Company other)
        {
            Id = Guid.NewGuid();
            Name = other.Name;
        }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public ICollection<Account> Accounts { get; set; }
    }
}