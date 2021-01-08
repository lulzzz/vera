using System;
using System.Collections.Generic;

namespace Vera.Models
{
    /// <summary>
    /// Company is the mother entity. E.g for a company like Coca Cola there would be 1 company
    /// "Coca Cola", but there can be multiple accounts to deal with local regulations. So
    /// "Coca Cola BV" for the Netherlands and then "Coca Cola Norway" for Norway, etc.
    /// </summary>
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
    }
}