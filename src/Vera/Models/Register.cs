using System;
using System.Collections.Generic;

namespace Vera.Models
{
    public class Register
    {
        public Register()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public string SystemId { get; set; }

        public Guid SupplierId { get; set; }

        public string Name { get; set; }

        public RegisterStatus Status { get; set; }

        public IDictionary<string, string> Data { get; set; }
    }
}
